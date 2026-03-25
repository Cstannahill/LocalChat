using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.Personas;

namespace LocalChat.Application.Inspection;

public sealed class PromptInspectionService : IPromptInspectionService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IUserPersonaRepository _userPersonaRepository;
    private readonly IModelProfileRepository _modelProfileRepository;
    private readonly IGenerationPresetRepository _generationPresetRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IRetrievalService _retrievalService;
    private readonly IModelContextService _modelContextService;
    private readonly IPromptComposer _promptComposer;

    public PromptInspectionService(
        ICharacterRepository characterRepository,
        IUserPersonaRepository userPersonaRepository,
        IModelProfileRepository modelProfileRepository,
        IGenerationPresetRepository generationPresetRepository,
        IConversationRepository conversationRepository,
        IRetrievalService retrievalService,
        IModelContextService modelContextService,
        IPromptComposer promptComposer)
    {
        _characterRepository = characterRepository;
        _userPersonaRepository = userPersonaRepository;
        _modelProfileRepository = modelProfileRepository;
        _generationPresetRepository = generationPresetRepository;
        _conversationRepository = conversationRepository;
        _retrievalService = retrievalService;
        _modelContextService = modelContextService;
        _promptComposer = promptComposer;
    }

    public async Task<ContextInspectionResult> InspectAsync(
        Guid characterId,
        Guid? conversationId,
        Guid? userPersonaId,
        string currentUserMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserMessage))
        {
            throw new ArgumentException("Inspection query cannot be empty.", nameof(currentUserMessage));
        }

        var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken)
            ?? throw new InvalidOperationException($"Character '{characterId}' was not found.");

        Conversation conversation;
        UserPersona? userPersona = null;

        if (conversationId.HasValue)
        {
            conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Conversation '{conversationId.Value}' was not found.");

            userPersona = conversation.UserPersona;
        }
        else
        {
            if (userPersonaId.HasValue)
            {
                userPersona = await _userPersonaRepository.GetByIdAsync(userPersonaId.Value, cancellationToken)
                    ?? throw new InvalidOperationException($"User persona '{userPersonaId.Value}' was not found.");
            }

            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                UserPersonaId = userPersona?.Id,
                UserPersona = userPersona,
                Title = "Inspection Preview",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

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

        var contextInfo = await _modelContextService.GetForModelAsync(
            modelProfile?.ModelIdentifier,
            modelProfile?.ContextWindow,
            cancellationToken);

        var retrievalInspection = await _retrievalService.InspectAsync(
            character.Id,
            conversationId,
            currentUserMessage,
            cancellationToken);

        var latestSummary = conversationId.HasValue
            ? await _conversationRepository.GetLatestSummaryAsync(conversation.Id, cancellationToken)
            : null;

        var orderedPriorMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        var rawMessages = GetRawMessagesAfterSummary(orderedPriorMessages, latestSummary);

        var trimmedRawMessages = TrimRawHistoryToFit(
            character,
            userPersona,
            conversation,
            retrievalInspection.SelectedMemories,
            retrievalInspection.SelectedLoreEntries,
            latestSummary?.SummaryText,
            rawMessages,
            currentUserMessage,
            contextInfo.MaxPromptTokens);

        var prompt = _promptComposer.Compose(new PromptCompositionContext
        {
            Character = character,
            UserPersona = userPersona,
            Conversation = conversation,
            PriorMessages = trimmedRawMessages,
            CurrentUserMessage = currentUserMessage,
            RollingSummary = latestSummary?.SummaryText,
            ExplicitMemories = retrievalInspection.SelectedMemories,
            RelevantLoreEntries = retrievalInspection.SelectedLoreEntries
        });

        var summaryUsedInPrompt = prompt.Sections.Any(x =>
            x.Name == "Rolling Summary" &&
            !string.IsNullOrWhiteSpace(x.Content));

        var totalPriorMessageCount = orderedPriorMessages.Count;
        var includedRawMessageCount = trimmedRawMessages.Count;
        var excludedRawMessageCount = Math.Max(0, totalPriorMessageCount - includedRawMessageCount);

        var summaryCoveredMessageCount = latestSummary is null
            ? 0
            : Math.Max(0, latestSummary.EndSequenceNumber - latestSummary.StartSequenceNumber + 1);

        return new ContextInspectionResult
        {
            Query = currentUserMessage,
            ModelName = contextInfo.ModelName,
            ModelProfileName = modelProfile?.Name,
            GenerationPresetName = generationPreset?.Name,
            EffectiveContextLength = contextInfo.EffectiveContextLength,
            MaxPromptTokens = contextInfo.MaxPromptTokens,
            EstimatedPromptTokens = prompt.EstimatedTokens,
            FitsWithinBudget = prompt.EstimatedTokens <= contextInfo.MaxPromptTokens,
            FinalPrompt = prompt.Prompt,
            Sections = prompt.Sections,
            SelectedSceneState = prompt.SelectedSceneState,
            SuppressedSceneState = prompt.SuppressedSceneState,
            SelectedDurableMemory = prompt.SelectedDurableMemory,
            SuppressedDurableMemory = prompt.SuppressedDurableMemory,
            SelectedMemories = retrievalInspection.SelectedMemories,
            SelectedLoreEntries = retrievalInspection.SelectedLoreEntries,
            RollingSummary = latestSummary?.SummaryText,
            SummaryUsedInPrompt = summaryUsedInPrompt,
            LatestSummaryCheckpointId = latestSummary?.Id,
            LatestSummaryCreatedAt = latestSummary?.CreatedAt,
            SummaryStartSequenceNumber = latestSummary?.StartSequenceNumber,
            SummaryEndSequenceNumber = latestSummary?.EndSequenceNumber,
            SummaryCoveredMessageCount = summaryCoveredMessageCount,
            TotalPriorMessageCount = totalPriorMessageCount,
            IncludedRawMessageCount = includedRawMessageCount,
            ExcludedRawMessageCount = excludedRawMessageCount
        };
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
        var workingMessages = rawMessages.OrderBy(x => x.SequenceNumber).ToList();

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

            if (prompt.EstimatedTokens <= maxPromptTokens)
            {
                return workingMessages;
            }

            if (workingMessages.Count == 0)
            {
                return workingMessages;
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
}
