using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Application.Runtime;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Generation;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Chat;

public sealed class ConversationContinuationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IModelProfileRepository _modelProfileRepository;
    private readonly IGenerationPresetRepository _generationPresetRepository;
    private readonly IPromptComposer _promptComposer;
    private readonly IInferenceProvider _inferenceProvider;
    private readonly IRetrievalService _retrievalService;
    private readonly IConversationBackgroundWorkScheduler _backgroundWorkScheduler;
    private readonly IRuntimeSelectionResolver? _runtimeSelectionResolver;
    private readonly IGenerationPromptSnapshotRepository? _generationPromptSnapshotRepository;

    public ConversationContinuationService(
        IConversationRepository conversationRepository,
        IModelProfileRepository modelProfileRepository,
        IGenerationPresetRepository generationPresetRepository,
        IPromptComposer promptComposer,
        IInferenceProvider inferenceProvider,
        IRetrievalService retrievalService,
        IConversationBackgroundWorkScheduler backgroundWorkScheduler,
        IRuntimeSelectionResolver? runtimeSelectionResolver = null,
        IGenerationPromptSnapshotRepository? generationPromptSnapshotRepository = null)
    {
        _conversationRepository = conversationRepository;
        _modelProfileRepository = modelProfileRepository;
        _generationPresetRepository = generationPresetRepository;
        _promptComposer = promptComposer;
        _inferenceProvider = inferenceProvider;
        _retrievalService = retrievalService;
        _backgroundWorkScheduler = backgroundWorkScheduler;
        _runtimeSelectionResolver = runtimeSelectionResolver;
        _generationPromptSnapshotRepository = generationPromptSnapshotRepository;
    }

    public async Task<ContinueConversationResult> ContinueConversationAsync(
        Guid conversationId,
        Func<string, CancellationToken, Task> onToken,
        string? overrideProvider = null,
        string? overrideModelIdentifier = null,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        if (orderedMessages.Count == 0)
        {
            throw new InvalidOperationException("Cannot continue a conversation that has no messages.");
        }

        var latestMessage = orderedMessages[^1];
        if (latestMessage.Role != MessageRole.Assistant)
        {
            throw new InvalidOperationException("Continue is only valid when the latest message is an assistant message.");
        }

        var retrievalQuery = BuildRetrievalQuery(orderedMessages);
        var retrieval = await _retrievalService.InspectAsync(
            conversation.CharacterId,
            conversation.Id,
            retrievalQuery,
            cancellationToken);

        var rollingSummary = conversation.SummaryCheckpoints
            .OrderByDescending(x => x.EndSequenceNumber)
            .FirstOrDefault()?.SummaryText;

        var character = conversation.Character
                        ?? throw new InvalidOperationException("Conversation character was not loaded.");

        var streamedAnyToken = false;
        var accumulated = new List<string>();
        var runtimeSelection = await ResolveRuntimeSelectionAsync(
            character,
            conversation,
            overrideProvider,
            overrideModelIdentifier,
            cancellationToken);
        var resolvedModelIdentifier = runtimeSelection.ModelIdentifier;

        var executionSettings = BuildExecutionSettings(runtimeSelection, resolvedModelIdentifier);

        var attempts = new List<ContinuationAttempt>
        {
            new(14, true, executionSettings),
            new(14, true, null),
            new(8, true, null),
            new(6, false, null)
        };

        string? generatedText = null;
        PromptCompositionResult? successfulPromptComposition = null;
        Exception? lastFailure = null;
        var generationStartedAt = DateTime.UtcNow;
        var resolvedContextWindow = runtimeSelection.ModelProfile?.ContextWindow;

        foreach (var attempt in attempts)
        {
            if (streamedAnyToken)
            {
                break;
            }

            var prompt = _promptComposer.Compose(new PromptCompositionContext
            {
                Character = character,
                Conversation = conversation,
                UserPersona = conversation.UserPersona,
                ExplicitMemories = attempt.IncludeRetrievalContext ? retrieval.SelectedMemories : Array.Empty<MemoryItem>(),
                RelevantLoreEntries = attempt.IncludeRetrievalContext ? retrieval.SelectedLoreEntries : Array.Empty<LoreEntry>(),
                RollingSummary = rollingSummary,
                PriorMessages = orderedMessages.TakeLast(attempt.PriorMessageCount).ToList(),
                CurrentUserMessage = null,
                ContinueWithoutUserMessage = true
            });

            try
            {
                generatedText = await _inferenceProvider.StreamCompletionAsync(
                    prompt.Prompt,
                    async (delta, ct) =>
                    {
                        if (!string.IsNullOrEmpty(delta))
                        {
                            streamedAnyToken = true;
                            successfulPromptComposition ??= prompt;
                            accumulated.Add(delta);
                            await onToken(delta, ct);
                        }
                    },
                    attempt.ExecutionSettings,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                lastFailure = ex;
                continue;
            }

            var attemptText = string.IsNullOrWhiteSpace(generatedText)
                ? string.Concat(accumulated).Trim()
                : generatedText.Trim();

            if (!string.IsNullOrWhiteSpace(attemptText))
            {
                generatedText = attemptText;
                successfulPromptComposition = prompt;
                break;
            }
        }

        var finalText = string.IsNullOrWhiteSpace(generatedText)
            ? string.Concat(accumulated).Trim()
            : generatedText.Trim();

        if (string.IsNullOrWhiteSpace(finalText))
        {
            if (lastFailure is not null)
            {
                throw new InvalidOperationException(
                    "Continuation generation failed after fallback attempts.",
                    lastFailure);
            }

            throw new InvalidOperationException("Continuation generation returned empty content.");
        }
        var generationCompletedAt = DateTime.UtcNow;

        var nextSequence = await _conversationRepository.GetNextSequenceNumberAsync(
            conversation.Id,
            cancellationToken);

        var assistantMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            OriginType = MessageOriginType.AssistantGenerated,
            Content = finalText,
            SequenceNumber = nextSequence,
            CreatedAt = DateTime.UtcNow,
            SelectedVariantIndex = 0
        };

        await _conversationRepository.AddMessageAsync(assistantMessage, cancellationToken);

        var provenance = AssistantGenerationProvenance.Create(
            runtimeSelection.ProviderType,
            resolvedModelIdentifier,
            runtimeSelection.ModelProfile?.Id,
            runtimeSelection.GenerationPreset?.Id,
            runtimeSelection.SourceType);
        var assistantVariant = AssistantMessageVariantFactory.Create(
            finalText,
            0,
            provenance,
            generationStartedAt,
            generationCompletedAt);
        assistantVariant.MessageId = assistantMessage.Id;

        await _conversationRepository.AddMessageVariantAsync(assistantVariant, cancellationToken);

        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        if (successfulPromptComposition is not null)
        {
            await PersistGenerationPromptSnapshotAsync(
                conversation.Id,
                assistantMessage.Id,
                assistantVariant.Id,
                successfulPromptComposition,
                provenance,
                resolvedContextWindow,
                cancellationToken);
        }

        await _backgroundWorkScheduler.ScheduleConversationChangeAsync(
            conversation.Id,
            ConversationBackgroundWorkType.All,
            "conversation-continue",
            cancellationToken);

        return new ContinueConversationResult
        {
            ConversationId = conversation.Id,
            AssistantMessageId = assistantMessage.Id,
            AssistantMessage = finalText
        };
    }

    private static string BuildRetrievalQuery(IReadOnlyList<Message> orderedMessages)
    {
        return string.Join(
            "\n",
            orderedMessages
                .TakeLast(6)
                .Select(x => $"{x.Role}: {x.Content}"));
    }

    private async Task<ResolvedRuntimeSelection> ResolveRuntimeSelectionAsync(
        Domain.Entities.Characters.Character character,
        Conversation conversation,
        string? overrideProvider,
        string? overrideModelIdentifier,
        CancellationToken cancellationToken)
    {
        if (_runtimeSelectionResolver is not null)
        {
            return await _runtimeSelectionResolver.ResolveAsync(
                character,
                conversation,
                overrideProvider,
                overrideModelIdentifier,
                cancellationToken);
        }

        var modelProfile = character.DefaultModelProfileId.HasValue
            ? await _modelProfileRepository.GetByIdAsync(character.DefaultModelProfileId.Value, cancellationToken)
            : null;

        var generationPreset = character.DefaultGenerationPresetId.HasValue
            ? await _generationPresetRepository.GetByIdAsync(character.DefaultGenerationPresetId.Value, cancellationToken)
            : null;

        var resolvedModelIdentifier = modelProfile?.ModelIdentifier;
        var turnOverrideModelIdentifier = BuildTurnOverrideModelIdentifier(
            overrideProvider,
            overrideModelIdentifier);

        RuntimeSourceType sourceType = RuntimeSourceType.CharacterDefault;
        ProviderType? providerType = modelProfile?.ProviderType;

        if (!string.IsNullOrWhiteSpace(turnOverrideModelIdentifier))
        {
            resolvedModelIdentifier = turnOverrideModelIdentifier;
            providerType = ModelRoute.Parse(turnOverrideModelIdentifier, ProviderType.Ollama).Provider;
            modelProfile = null;
            sourceType = RuntimeSourceType.OneTurnOverride;
        }

        if (modelProfile is null && generationPreset is null && sourceType != RuntimeSourceType.OneTurnOverride)
        {
            sourceType = RuntimeSourceType.ProviderDefault;
        }

        return new ResolvedRuntimeSelection
        {
            SourceType = sourceType,
            ProviderType = providerType,
            ModelIdentifier = resolvedModelIdentifier,
            ModelProfile = modelProfile,
            GenerationPreset = generationPreset
        };
    }

    private static InferenceExecutionSettings? BuildExecutionSettings(
        ResolvedRuntimeSelection runtimeSelection,
        string? modelIdentifierOverride = null)
    {
        if (runtimeSelection.ModelProfile is null && runtimeSelection.GenerationPreset is null)
        {
            return null;
        }

        return new InferenceExecutionSettings
        {
            ModelIdentifier = string.IsNullOrWhiteSpace(modelIdentifierOverride)
                ? runtimeSelection.ModelProfile?.ModelIdentifier
                : modelIdentifierOverride,
            ContextWindow = runtimeSelection.ModelProfile?.ContextWindow,
            MaxOutputTokens = runtimeSelection.GenerationPreset?.MaxOutputTokens > 0
                ? runtimeSelection.GenerationPreset.MaxOutputTokens
                : null,
            Temperature = runtimeSelection.GenerationPreset?.Temperature,
            TopP = runtimeSelection.GenerationPreset?.TopP,
            RepeatPenalty = runtimeSelection.GenerationPreset?.RepeatPenalty,
            StopSequences = ParseStopSequences(runtimeSelection.GenerationPreset?.StopSequencesText)
        };
    }

    private static string? BuildTurnOverrideModelIdentifier(
        string? overrideProvider,
        string? overrideModelIdentifier)
    {
        if (string.IsNullOrWhiteSpace(overrideModelIdentifier))
        {
            return null;
        }

        var defaultProvider = ProviderType.Ollama;

        if (!string.IsNullOrWhiteSpace(overrideProvider)
            && ModelRoute.TryParseProvider(overrideProvider, out var parsedProvider))
        {
            defaultProvider = parsedProvider;
        }

        return ModelRoute.NormalizeForStorage(defaultProvider, overrideModelIdentifier);
    }

    private static IReadOnlyList<string> ParseStopSequences(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string BuildPromptSectionsJson(PromptCompositionResult promptCompositionResult)
    {
        var payload = promptCompositionResult.Sections
            .Select(x => new PromptSectionSnapshotRecord
            {
                Name = x.Name,
                Content = x.Content,
                EstimatedTokens = x.EstimatedTokens
            })
            .ToList();

        return JsonSerializer.Serialize(payload);
    }

    private static int CalculateEstimatedPromptTokens(PromptCompositionResult promptCompositionResult)
    {
        return promptCompositionResult.Sections.Sum(x => x.EstimatedTokens);
    }

    private async Task PersistGenerationPromptSnapshotAsync(
        Guid conversationId,
        Guid messageId,
        Guid messageVariantId,
        PromptCompositionResult promptCompositionResult,
        AssistantGenerationProvenance provenance,
        int? resolvedContextWindow,
        CancellationToken cancellationToken)
    {
        if (_generationPromptSnapshotRepository is null)
        {
            return;
        }

        var snapshot = new GenerationPromptSnapshot
        {
            Id = Guid.NewGuid(),
            MessageVariantId = messageVariantId,
            MessageId = messageId,
            ConversationId = conversationId,
            FullPromptText = promptCompositionResult.Prompt,
            PromptSectionsJson = BuildPromptSectionsJson(promptCompositionResult),
            EstimatedPromptTokens = CalculateEstimatedPromptTokens(promptCompositionResult),
            ResolvedContextWindow = resolvedContextWindow,
            ProviderType = provenance.ProviderType,
            ModelIdentifier = provenance.ModelIdentifier,
            ModelProfileId = provenance.ModelProfileId,
            GenerationPresetId = provenance.GenerationPresetId,
            RuntimeSourceType = provenance.RuntimeSourceType,
            CreatedAt = DateTime.UtcNow
        };

        await _generationPromptSnapshotRepository.AddAsync(snapshot, cancellationToken);
        await _generationPromptSnapshotRepository.SaveChangesAsync(cancellationToken);
    }

    private sealed class PromptSectionSnapshotRecord
    {
        public string Name { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int EstimatedTokens { get; set; }
    }

    private sealed record ContinuationAttempt(
        int PriorMessageCount,
        bool IncludeRetrievalContext,
        InferenceExecutionSettings? ExecutionSettings);
}
