using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Abstractions.Telemetry;
using LocalChat.Application.Background;
using LocalChat.Application.Features.Chat.SendChatMessage;
using LocalChat.Application.Features.Summaries;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Application.Runtime;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Generation;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.Personas;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Chat;

public sealed class ChatOrchestrator
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IUserPersonaRepository _userPersonaRepository;
    private readonly IModelProfileRepository _modelProfileRepository;
    private readonly IGenerationPresetRepository _generationPresetRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IRetrievalService _retrievalService;
    private readonly IPromptComposer _promptComposer;
    private readonly IInferenceProvider _inferenceProvider;
    private readonly ITokenEstimator _tokenEstimator;
    private readonly IModelContextService _modelContextService;
    private readonly IConversationSummaryService _conversationSummaryService;
    private readonly IConversationBackgroundWorkScheduler _backgroundWorkScheduler;
    private readonly SummaryOptions _summaryOptions;
    private readonly IRuntimeSelectionResolver? _runtimeSelectionResolver;
    private readonly IGenerationPromptSnapshotRepository? _generationPromptSnapshotRepository;
    private readonly IRequestFlowTiming _requestFlowTiming;

    public ChatOrchestrator(
        ICharacterRepository characterRepository,
        IUserPersonaRepository userPersonaRepository,
        IModelProfileRepository modelProfileRepository,
        IGenerationPresetRepository generationPresetRepository,
        IConversationRepository conversationRepository,
        IRetrievalService retrievalService,
        IPromptComposer promptComposer,
        IInferenceProvider inferenceProvider,
        ITokenEstimator tokenEstimator,
        IModelContextService modelContextService,
        IConversationSummaryService conversationSummaryService,
        IConversationBackgroundWorkScheduler backgroundWorkScheduler,
        SummaryOptions summaryOptions,
        IRuntimeSelectionResolver? runtimeSelectionResolver = null,
        IGenerationPromptSnapshotRepository? generationPromptSnapshotRepository = null,
        IRequestFlowTiming? requestFlowTiming = null)
    {
        _characterRepository = characterRepository;
        _userPersonaRepository = userPersonaRepository;
        _modelProfileRepository = modelProfileRepository;
        _generationPresetRepository = generationPresetRepository;
        _conversationRepository = conversationRepository;
        _retrievalService = retrievalService;
        _promptComposer = promptComposer;
        _inferenceProvider = inferenceProvider;
        _tokenEstimator = tokenEstimator;
        _modelContextService = modelContextService;
        _conversationSummaryService = conversationSummaryService;
        _backgroundWorkScheduler = backgroundWorkScheduler;
        _summaryOptions = summaryOptions;
        _runtimeSelectionResolver = runtimeSelectionResolver;
        _generationPromptSnapshotRepository = generationPromptSnapshotRepository;
        _requestFlowTiming = requestFlowTiming ?? NullRequestFlowTiming.Instance;
    }

    public async Task<SendChatMessageResult> SendAsync(
        SendChatMessageCommand command,
        Func<string, CancellationToken, Task> onDelta,
        string? overrideProvider = null,
        string? overrideModelIdentifier = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Message))
        {
            throw new ArgumentException("Message cannot be empty.", nameof(command));
        }

        _requestFlowTiming.AddTag("characterId", command.CharacterId.ToString());

        Character character;
        using (_requestFlowTiming.BeginStage("chat.resolve_character"))
        {
            character = await _characterRepository.GetByIdAsync(command.CharacterId, cancellationToken)
                ?? throw new InvalidOperationException($"Character '{command.CharacterId}' was not found.");
        }

        var conversationCreated = false;
        Conversation conversation;
        UserPersona? userPersona = null;

        using (_requestFlowTiming.BeginStage("chat.resolve_conversation"))
        {
            if (command.ConversationId.HasValue)
            {
                conversation = await _conversationRepository.GetByIdWithMessagesAsync(
                                   command.ConversationId.Value,
                                   cancellationToken)
                               ?? throw new InvalidOperationException(
                                   $"Conversation '{command.ConversationId.Value}' was not found.");

                if (conversation.CharacterId != command.CharacterId)
                {
                    throw new InvalidOperationException(
                        "Conversation does not belong to the requested character.");
                }

                userPersona = conversation.UserPersona;
            }
            else
            {
                if (command.UserPersonaId.HasValue)
                {
                    userPersona = await _userPersonaRepository.GetByIdAsync(command.UserPersonaId.Value, cancellationToken)
                        ?? throw new InvalidOperationException(
                            $"User persona '{command.UserPersonaId.Value}' was not found.");
                }

                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    Character = character,
                    UserPersonaId = userPersona?.Id,
                    UserPersona = userPersona,
                    Title = BuildConversationTitle(command.Message, character.Name),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _conversationRepository.AddAsync(conversation, cancellationToken);
                conversationCreated = true;
            }
        }

        _requestFlowTiming.AddTag("conversationId", conversation.Id.ToString());
        _requestFlowTiming.AddTag("conversationCreated", conversationCreated.ToString());

        ConversationMessageSeeder.SeedGreetingIfNeeded(conversation);

        ResolvedRuntimeSelection runtimeSelection;
        using (_requestFlowTiming.BeginStage("chat.resolve_runtime"))
        {
            runtimeSelection = await ResolveRuntimeSelectionAsync(
                character,
                conversation,
                overrideProvider,
                overrideModelIdentifier,
                cancellationToken);
        }
        var resolvedModelIdentifier = runtimeSelection.ModelIdentifier;
        var resolvedModelProfile = runtimeSelection.ModelProfile;
        _requestFlowTiming.AddTag("provider", runtimeSelection.ProviderType?.ToString());
        _requestFlowTiming.AddTag("modelIdentifier", resolvedModelIdentifier);

        var contextInfo = default(ModelContextInfo);
        using (_requestFlowTiming.BeginStage("chat.resolve_model_context"))
        {
            contextInfo = await _modelContextService.GetForModelAsync(
                resolvedModelIdentifier,
                resolvedModelProfile?.ContextWindow,
                cancellationToken);
        }

        var priorMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        var retrievalInspection = default(LocalChat.Application.Inspection.RetrievalInspectionResult);
        using (_requestFlowTiming.BeginStage("chat.retrieval.inspect"))
        {
            retrievalInspection = await _retrievalService.InspectAsync(
                character.Id,
                conversation.Id,
                command.Message,
                cancellationToken);
        }

        var explicitMemories = retrievalInspection.SelectedMemories;
        var relevantLoreEntries = retrievalInspection.SelectedLoreEntries;

        SummaryCheckpoint? latestSummary;
        using (_requestFlowTiming.BeginStage("chat.summary.load_latest"))
        {
            latestSummary = await _conversationRepository.GetLatestSummaryAsync(
                conversation.Id,
                cancellationToken);
        }

        using (_requestFlowTiming.BeginStage("chat.summary.ensure_rolling"))
        {
            latestSummary = await EnsureRollingSummaryIfNeededAsync(
                character,
                userPersona,
                conversation,
                explicitMemories,
                relevantLoreEntries,
                priorMessages,
                latestSummary,
                command.Message,
                contextInfo.MaxPromptTokens,
                cancellationToken);
        }

        var rawMessages = GetRawMessagesAfterSummary(priorMessages, latestSummary);

        List<Message> trimmedRawMessages;
        using (_requestFlowTiming.BeginStage("chat.history.trim_to_fit"))
        {
            trimmedRawMessages = TrimRawHistoryToFit(
                character,
                userPersona,
                conversation,
                explicitMemories,
                relevantLoreEntries,
                latestSummary?.SummaryText,
                rawMessages,
                command.Message,
                contextInfo.MaxPromptTokens).ToList();
        }

        var userSequenceNumber = conversation.Messages.Count == 0
            ? 1
            : conversation.Messages.Max(x => x.SequenceNumber) + 1;

        var userMessageContent = command.Message.Trim();

        var userMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            OriginType = MessageOriginType.User,
            Content = userMessageContent,
            SequenceNumber = userSequenceNumber,
            CreatedAt = DateTime.UtcNow,
            SelectedVariantIndex = null
        };

        using (_requestFlowTiming.BeginStage("chat.persist.user_message"))
        {
            await _conversationRepository.AddMessageAsync(userMessage, cancellationToken);
        }

        PromptCompositionResult finalPrompt;
        using (_requestFlowTiming.BeginStage("chat.prompt.compose"))
        {
            finalPrompt = _promptComposer.Compose(new PromptCompositionContext
            {
                Character = character,
                UserPersona = userPersona,
                Conversation = conversation,
                PriorMessages = trimmedRawMessages,
                CurrentUserMessage = command.Message,
                RollingSummary = latestSummary?.SummaryText,
                ExplicitMemories = explicitMemories,
                RelevantLoreEntries = relevantLoreEntries
            });
        }

        var generationStartedAt = DateTime.UtcNow;
        string assistantText;
        using (_requestFlowTiming.BeginStage("chat.inference.stream"))
        {
            assistantText = await _inferenceProvider.StreamCompletionAsync(
                finalPrompt.Prompt,
                onDelta,
                BuildExecutionSettings(runtimeSelection, resolvedModelIdentifier),
                cancellationToken);
        }
        var generationCompletedAt = DateTime.UtcNow;

        var provenance = BuildAssistantGenerationProvenance(runtimeSelection);

        var assistantMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            OriginType = MessageOriginType.AssistantGenerated,
            Content = assistantText,
            SequenceNumber = userSequenceNumber + 1,
            CreatedAt = DateTime.UtcNow,
            SelectedVariantIndex = 0
        };

        using (_requestFlowTiming.BeginStage("chat.persist.assistant_message"))
        {
            await _conversationRepository.AddMessageAsync(assistantMessage, cancellationToken);
        }

        var initialVariant = AssistantMessageVariantFactory.Create(
            assistantText,
            0,
            provenance,
            generationStartedAt,
            generationCompletedAt);
        initialVariant.MessageId = assistantMessage.Id;

        using (_requestFlowTiming.BeginStage("chat.persist.assistant_variant"))
        {
            await _conversationRepository.AddMessageVariantAsync(initialVariant, cancellationToken);
        }

        conversation.UpdatedAt = DateTime.UtcNow;

        using (_requestFlowTiming.BeginStage("chat.persist.save_changes"))
        {
            await _conversationRepository.SaveChangesAsync(cancellationToken);
        }

        using (_requestFlowTiming.BeginStage("chat.persist.prompt_snapshot"))
        {
            await PersistGenerationPromptSnapshotAsync(
                conversation.Id,
                assistantMessage.Id,
                initialVariant.Id,
                finalPrompt,
                provenance,
                contextInfo.EffectiveContextLength,
                cancellationToken);
        }

        using (_requestFlowTiming.BeginStage("chat.background.schedule"))
        {
            await _backgroundWorkScheduler.ScheduleConversationChangeAsync(
                conversation.Id,
                ConversationBackgroundWorkType.All,
                "chat-turn-complete",
                cancellationToken);
        }

        return new SendChatMessageResult
        {
            ConversationId = conversation.Id,
            UserMessageId = userMessage.Id,
            AssistantMessageId = assistantMessage.Id,
            AssistantMessage = assistantText,
            ConversationCreated = conversationCreated
        };
    }

    public async Task<RegenerateAssistantMessageResult> RegenerateLatestAssistantMessageAsync(
        Guid conversationId,
        Guid assistantMessageId,
        string? overrideProvider = null,
        string? overrideModelIdentifier = null,
        CancellationToken cancellationToken = default)
    {
        _requestFlowTiming.AddTag("conversationId", conversationId.ToString());

        Conversation conversation;
        using (_requestFlowTiming.BeginStage("chat.regenerate.load_conversation"))
        {
            conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");
        }

        _requestFlowTiming.AddTag("characterId", conversation.CharacterId.ToString());

        Character character;
        using (_requestFlowTiming.BeginStage("chat.regenerate.load_character"))
        {
            character = conversation.Character
                ?? await _characterRepository.GetByIdAsync(conversation.CharacterId, cancellationToken)
                ?? throw new InvalidOperationException($"Character '{conversation.CharacterId}' was not found.");
        }

        var userPersona = conversation.UserPersona;
        ResolvedRuntimeSelection runtimeSelection;
        using (_requestFlowTiming.BeginStage("chat.regenerate.resolve_runtime"))
        {
            runtimeSelection = await ResolveRuntimeSelectionAsync(
                character,
                conversation,
                overrideProvider,
                overrideModelIdentifier,
                cancellationToken);
        }
        var resolvedModelIdentifier = runtimeSelection.ModelIdentifier;
        var resolvedModelProfile = runtimeSelection.ModelProfile;
        _requestFlowTiming.AddTag("provider", runtimeSelection.ProviderType?.ToString());
        _requestFlowTiming.AddTag("modelIdentifier", resolvedModelIdentifier);

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        if (orderedMessages.Count == 0)
        {
            throw new InvalidOperationException("Conversation has no messages.");
        }

        var assistantIndex = orderedMessages.FindIndex(x => x.Id == assistantMessageId);
        if (assistantIndex < 0)
        {
            throw new InvalidOperationException($"Assistant message '{assistantMessageId}' was not found.");
        }

        var assistantMessage = orderedMessages[assistantIndex];
        if (assistantMessage.Role != MessageRole.Assistant)
        {
            throw new InvalidOperationException("Only assistant messages can be regenerated.");
        }

        if (assistantIndex != orderedMessages.Count - 1)
        {
            throw new InvalidOperationException("Only the latest assistant message can be regenerated.");
        }

        if (assistantIndex == 0)
        {
            throw new InvalidOperationException("Assistant message has no preceding context and cannot be regenerated.");
        }

        var messagesBeforeAssistant = orderedMessages
            .Take(assistantIndex)
            .ToList();

        var precedingMessage = messagesBeforeAssistant[^1];
        var isContinuationRegeneration = precedingMessage.Role == MessageRole.Assistant;
        var executionSettings = BuildExecutionSettings(runtimeSelection, resolvedModelIdentifier);

        PromptCompositionResult finalPrompt;
        int? resolvedContextWindow = runtimeSelection.ModelProfile?.ContextWindow;

        if (isContinuationRegeneration)
        {
            var retrievalInspection = default(LocalChat.Application.Inspection.RetrievalInspectionResult);
            using (_requestFlowTiming.BeginStage("chat.regenerate.retrieval.inspect"))
            {
                retrievalInspection = await _retrievalService.InspectAsync(
                    character.Id,
                    conversation.Id,
                    BuildRetrievalQuery(messagesBeforeAssistant),
                    cancellationToken);
            }

            SummaryCheckpoint? latestSummary;
            using (_requestFlowTiming.BeginStage("chat.regenerate.summary.load_latest"))
            {
                latestSummary = await _conversationRepository.GetLatestSummaryAsync(conversation.Id, cancellationToken);
            }

            using (_requestFlowTiming.BeginStage("chat.regenerate.prompt.compose"))
            {
                finalPrompt = _promptComposer.Compose(new PromptCompositionContext
                {
                    Character = character,
                    UserPersona = userPersona,
                    Conversation = conversation,
                    PriorMessages = messagesBeforeAssistant.TakeLast(14).ToList(),
                    CurrentUserMessage = null,
                    ContinueWithoutUserMessage = true,
                    RollingSummary = latestSummary?.SummaryText,
                    ExplicitMemories = retrievalInspection.SelectedMemories,
                    RelevantLoreEntries = retrievalInspection.SelectedLoreEntries
                });
            }
        }
        else
        {
            var targetUserMessage = messagesBeforeAssistant
                .LastOrDefault(x => x.Role == MessageRole.User);

            if (targetUserMessage is null)
            {
                throw new InvalidOperationException("Assistant message is not paired with any preceding user message.");
            }

            ModelContextInfo contextInfo;
            using (_requestFlowTiming.BeginStage("chat.regenerate.resolve_model_context"))
            {
                contextInfo = await _modelContextService.GetForModelAsync(
                    resolvedModelIdentifier,
                    resolvedModelProfile?.ContextWindow,
                    cancellationToken);
            }
            resolvedContextWindow = contextInfo.EffectiveContextLength;

            var retrievalInspection = default(LocalChat.Application.Inspection.RetrievalInspectionResult);
            using (_requestFlowTiming.BeginStage("chat.regenerate.retrieval.inspect"))
            {
                retrievalInspection = await _retrievalService.InspectAsync(
                    character.Id,
                    conversation.Id,
                    targetUserMessage.Content,
                    cancellationToken);
            }

            var explicitMemories = retrievalInspection.SelectedMemories;
            var relevantLoreEntries = retrievalInspection.SelectedLoreEntries;

            SummaryCheckpoint? latestSummary;
            using (_requestFlowTiming.BeginStage("chat.regenerate.summary.load_latest"))
            {
                latestSummary = await _conversationRepository.GetLatestSummaryAsync(conversation.Id, cancellationToken);
            }

            var messagesBeforeTargetUser = orderedMessages
                .Where(x => x.SequenceNumber < targetUserMessage.SequenceNumber)
                .OrderBy(x => x.SequenceNumber)
                .ToList();

            var rawMessages = GetRawMessagesAfterSummary(messagesBeforeTargetUser, latestSummary);

            var trimmedRawMessages = TrimRawHistoryToFit(
                character,
                userPersona,
                conversation,
                explicitMemories,
                relevantLoreEntries,
                latestSummary?.SummaryText,
                rawMessages,
                targetUserMessage.Content,
                contextInfo.MaxPromptTokens);

            using (_requestFlowTiming.BeginStage("chat.regenerate.prompt.compose"))
            {
                finalPrompt = _promptComposer.Compose(new PromptCompositionContext
                {
                    Character = character,
                    UserPersona = userPersona,
                    Conversation = conversation,
                    PriorMessages = trimmedRawMessages,
                    CurrentUserMessage = targetUserMessage.Content,
                    RollingSummary = latestSummary?.SummaryText,
                    ExplicitMemories = explicitMemories,
                    RelevantLoreEntries = relevantLoreEntries
                });
            }
        }

        var generationStartedAt = DateTime.UtcNow;
        string regeneratedText;
        using (_requestFlowTiming.BeginStage("chat.regenerate.inference.stream"))
        {
            regeneratedText = await _inferenceProvider.StreamCompletionAsync(
                finalPrompt.Prompt,
                static (_, _) => Task.CompletedTask,
                executionSettings,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(regeneratedText) && executionSettings is not null)
        {
            using (_requestFlowTiming.BeginStage("chat.regenerate.inference.retry_default_settings"))
            {
                regeneratedText = await _inferenceProvider.StreamCompletionAsync(
                    finalPrompt.Prompt,
                    static (_, _) => Task.CompletedTask,
                    null,
                    cancellationToken);
            }
        }

        if (string.IsNullOrWhiteSpace(regeneratedText))
        {
            throw new InvalidOperationException("Regeneration returned empty content.");
        }
        var generationCompletedAt = DateTime.UtcNow;

        var provenance = BuildAssistantGenerationProvenance(runtimeSelection);

        var nextVariantIndex = assistantMessage.Variants.Count == 0
            ? 0
            : assistantMessage.Variants.Max(x => x.VariantIndex) + 1;

        var newVariant = AssistantMessageVariantFactory.Create(
            regeneratedText,
            nextVariantIndex,
            provenance,
            generationStartedAt,
            generationCompletedAt);
        newVariant.MessageId = assistantMessage.Id;

        using (_requestFlowTiming.BeginStage("chat.regenerate.persist.variant"))
        {
            await _conversationRepository.AddMessageVariantAsync(newVariant, cancellationToken);
        }

        // Works whether the ORM updates assistantMessage.Variants immediately or not.
        var variantCount = Math.Max(assistantMessage.Variants.Count, nextVariantIndex + 1);

        assistantMessage.Content = regeneratedText;
        assistantMessage.SelectedVariantIndex = nextVariantIndex;
        conversation.UpdatedAt = DateTime.UtcNow;

        using (_requestFlowTiming.BeginStage("chat.regenerate.persist.save_changes"))
        {
            await _conversationRepository.SaveChangesAsync(cancellationToken);
        }

        using (_requestFlowTiming.BeginStage("chat.regenerate.persist.prompt_snapshot"))
        {
            await PersistGenerationPromptSnapshotAsync(
                conversation.Id,
                assistantMessage.Id,
                newVariant.Id,
                finalPrompt,
                provenance,
                resolvedContextWindow,
                cancellationToken);
        }

        return new RegenerateAssistantMessageResult
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            AssistantMessage = regeneratedText,
            SelectedVariantIndex = nextVariantIndex,
            VariantCount = variantCount
        };
    }

    public async Task<SelectMessageVariantResult> SelectMessageVariantAsync(
        Guid conversationId,
        Guid assistantMessageId,
        int variantIndex,
        CancellationToken cancellationToken = default)
    {
        _requestFlowTiming.AddTag("conversationId", conversationId.ToString());

        Conversation conversation;
        using (_requestFlowTiming.BeginStage("chat.select_variant.load_conversation"))
        {
            conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");
        }

        var assistantMessage = conversation.Messages.FirstOrDefault(x => x.Id == assistantMessageId)
                               ?? throw new InvalidOperationException($"Assistant message '{assistantMessageId}' was not found.");

        if (assistantMessage.Role != MessageRole.Assistant)
        {
            throw new InvalidOperationException("Only assistant messages can select variants.");
        }

        var selectedVariant = assistantMessage.Variants.FirstOrDefault(x => x.VariantIndex == variantIndex)
                              ?? throw new InvalidOperationException($"Variant '{variantIndex}' was not found.");

        assistantMessage.Content = selectedVariant.Content;
        assistantMessage.SelectedVariantIndex = selectedVariant.VariantIndex;
        conversation.UpdatedAt = DateTime.UtcNow;

        using (_requestFlowTiming.BeginStage("chat.select_variant.save_changes"))
        {
            await _conversationRepository.SaveChangesAsync(cancellationToken);
        }

        return new SelectMessageVariantResult
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            AssistantMessage = assistantMessage.Content,
            SelectedVariantIndex = selectedVariant.VariantIndex,
            VariantCount = assistantMessage.Variants.Count
        };
    }

    private async Task<ResolvedRuntimeSelection> ResolveRuntimeSelectionAsync(
        Character character,
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

        var legacy = await ResolveCharacterRuntimeSelectionAsync(character, cancellationToken);
        var resolvedModelIdentifier = legacy.ModelProfile?.ModelIdentifier;
        var turnOverrideModelIdentifier = BuildTurnOverrideModelIdentifier(
            overrideProvider,
            overrideModelIdentifier);

        RuntimeSourceType sourceType = RuntimeSourceType.CharacterDefault;
        ProviderType? providerType = legacy.ModelProfile?.ProviderType;
        ModelProfile? modelProfile = legacy.ModelProfile;

        if (!string.IsNullOrWhiteSpace(turnOverrideModelIdentifier))
        {
            resolvedModelIdentifier = turnOverrideModelIdentifier;
            providerType = ModelRoute.Parse(turnOverrideModelIdentifier, ProviderType.Ollama).Provider;
            modelProfile = null;
            sourceType = RuntimeSourceType.OneTurnOverride;
        }

        if (legacy.ModelProfile is null && legacy.GenerationPreset is null && sourceType != RuntimeSourceType.OneTurnOverride)
        {
            sourceType = RuntimeSourceType.ProviderDefault;
        }

        return new ResolvedRuntimeSelection
        {
            SourceType = sourceType,
            ProviderType = providerType,
            ModelIdentifier = resolvedModelIdentifier,
            ModelProfile = modelProfile,
            GenerationPreset = legacy.GenerationPreset
        };
    }

    private async Task<(ModelProfile? ModelProfile, GenerationPreset? GenerationPreset)> ResolveCharacterRuntimeSelectionAsync(
        Character character,
        CancellationToken cancellationToken)
    {
        ModelProfile? modelProfile = null;
        if (character.DefaultModelProfileId.HasValue)
        {
            modelProfile = await _modelProfileRepository.GetByIdAsync(character.DefaultModelProfileId.Value, cancellationToken);
        }

        GenerationPreset? generationPreset = null;
        if (character.DefaultGenerationPresetId.HasValue)
        {
            generationPreset = await _generationPresetRepository.GetByIdAsync(character.DefaultGenerationPresetId.Value, cancellationToken);
        }

        return (modelProfile, generationPreset);
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
            MaxOutputTokens = runtimeSelection.GenerationPreset?.MaxOutputTokens,
            Temperature = runtimeSelection.GenerationPreset?.Temperature,
            TopP = runtimeSelection.GenerationPreset?.TopP,
            RepeatPenalty = runtimeSelection.GenerationPreset?.RepeatPenalty,
            StopSequences = ParseStopSequences(runtimeSelection.GenerationPreset?.StopSequencesText)
        };
    }

    private static AssistantGenerationProvenance BuildAssistantGenerationProvenance(
        ResolvedRuntimeSelection runtimeSelection)
    {
        return AssistantGenerationProvenance.Create(
            runtimeSelection.ProviderType,
            runtimeSelection.ModelIdentifier,
            runtimeSelection.ModelProfile?.Id,
            runtimeSelection.GenerationPreset?.Id,
            runtimeSelection.SourceType);
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

    private sealed class PromptSectionSnapshotRecord
    {
        public string Name { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int EstimatedTokens { get; set; }
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

    private async Task<SummaryCheckpoint?> EnsureRollingSummaryIfNeededAsync(
        Character character,
        UserPersona? userPersona,
        Conversation conversation,
        IReadOnlyList<MemoryItem> explicitMemories,
        IReadOnlyList<LoreEntry> relevantLoreEntries,
        IReadOnlyList<Message> priorMessages,
        SummaryCheckpoint? latestSummary,
        string currentUserMessage,
        int maxPromptTokens,
        CancellationToken cancellationToken)
    {
        if (!_summaryOptions.Enabled || priorMessages.Count == 0)
        {
            return latestSummary;
        }

        var rawMessages = GetRawMessagesAfterSummary(priorMessages, latestSummary);

        var promptWithCurrentState = _promptComposer.Compose(new PromptCompositionContext
        {
            Character = character,
            UserPersona = userPersona,
            Conversation = conversation,
            PriorMessages = rawMessages,
            CurrentUserMessage = currentUserMessage,
            RollingSummary = latestSummary?.SummaryText,
            ExplicitMemories = explicitMemories,
            RelevantLoreEntries = relevantLoreEntries
        });

        var estimatedTokens = _tokenEstimator.EstimateTokens(promptWithCurrentState.Prompt);
        if (estimatedTokens <= maxPromptTokens)
        {
            return latestSummary;
        }

        if (rawMessages.Count <= _summaryOptions.KeepRecentMessageCount)
        {
            return latestSummary;
        }

        var summarizeCount = Math.Min(
            rawMessages.Count - _summaryOptions.KeepRecentMessageCount,
            _summaryOptions.MaxMessagesPerPass);

        if (summarizeCount < _summaryOptions.MinMessagesToSummarize)
        {
            return latestSummary;
        }

        var messagesToSummarize = rawMessages
            .OrderBy(x => x.SequenceNumber)
            .Take(summarizeCount)
            .ToList();

        var newSummaryText = await _conversationSummaryService.BuildRollingSummaryAsync(
            latestSummary?.SummaryText,
            messagesToSummarize,
            cancellationToken);

        var newCheckpoint = new SummaryCheckpoint
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            StartSequenceNumber = latestSummary?.StartSequenceNumber ?? messagesToSummarize.First().SequenceNumber,
            EndSequenceNumber = messagesToSummarize.Last().SequenceNumber,
            SummaryText = newSummaryText,
            CreatedAt = DateTime.UtcNow,
            ReplacedByCheckpointId = null
        };

        await _conversationRepository.AddSummaryCheckpointAsync(newCheckpoint, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return newCheckpoint;
    }

    private IReadOnlyList<Message> TrimRawHistoryToFit(
        Character character,
        UserPersona? userPersona,
        Conversation conversation,
        IReadOnlyList<MemoryItem> explicitMemories,
        IReadOnlyList<LoreEntry> relevantLoreEntries,
        string? rollingSummary,
        IReadOnlyList<Message> rawMessages,
        string currentUserMessage,
        int maxPromptTokens)
    {
        var workingMessages = rawMessages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        while (true)
        {
            var prompt = _promptComposer.Compose(new PromptCompositionContext
            {
                Character = character,
                UserPersona = userPersona,
                Conversation = conversation,
                PriorMessages = workingMessages,
                CurrentUserMessage = currentUserMessage,
                RollingSummary = rollingSummary,
                ExplicitMemories = explicitMemories,
                RelevantLoreEntries = relevantLoreEntries
            });

            var estimatedTokens = _tokenEstimator.EstimateTokens(prompt.Prompt);

            if (estimatedTokens <= maxPromptTokens)
            {
                return workingMessages;
            }

            if (workingMessages.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Prompt still exceeds max prompt budget after trimming. Estimated tokens: {estimatedTokens}, max prompt tokens: {maxPromptTokens}.");
            }

            workingMessages.RemoveAt(0);
        }
    }

    private static List<Message> GetRawMessagesAfterSummary(
        IReadOnlyList<Message> priorMessages,
        SummaryCheckpoint? latestSummary)
    {
        if (latestSummary is null)
        {
            return priorMessages.OrderBy(x => x.SequenceNumber).ToList();
        }

        return priorMessages
            .Where(x => x.SequenceNumber > latestSummary.EndSequenceNumber)
            .OrderBy(x => x.SequenceNumber)
            .ToList();
    }

    private static string BuildRetrievalQuery(IReadOnlyList<Message> orderedMessages)
    {
        return string.Join(
            "\n",
            orderedMessages
                .TakeLast(6)
                .Select(x => $"{x.Role}: {x.Content}"));
    }

    private static string BuildConversationTitle(string message, string characterName)
    {
        var trimmed = message.Trim();
        if (trimmed.Length <= 50)
        {
            return $"{characterName}: {trimmed}";
        }

        return $"{characterName}: {trimmed[..50]}...";
    }
}
