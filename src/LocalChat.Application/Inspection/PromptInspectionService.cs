using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.UserProfiles;

namespace LocalChat.Application.Inspection;

public sealed class PromptInspectionService : IPromptInspectionService
{
    private readonly IAgentRepository _agentRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IModelProfileRepository _modelProfileRepository;
    private readonly IGenerationPresetRepository _generationPresetRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IRetrievalService _retrievalService;
    private readonly IModelContextService _modelContextService;
    private readonly IPromptComposer _promptComposer;

    public PromptInspectionService(
        IAgentRepository agentRepository,
        IUserProfileRepository userProfileRepository,
        IModelProfileRepository modelProfileRepository,
        IGenerationPresetRepository generationPresetRepository,
        IConversationRepository conversationRepository,
        IRetrievalService retrievalService,
        IModelContextService modelContextService,
        IPromptComposer promptComposer
    )
    {
        _agentRepository = agentRepository;
        _userProfileRepository = userProfileRepository;
        _modelProfileRepository = modelProfileRepository;
        _generationPresetRepository = generationPresetRepository;
        _conversationRepository = conversationRepository;
        _retrievalService = retrievalService;
        _modelContextService = modelContextService;
        _promptComposer = promptComposer;
    }

    public async Task<ContextInspectionResult> InspectAsync(
        Guid agentId,
        Guid? conversationId,
        Guid? userProfileId,
        string currentUserMessage,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(currentUserMessage))
        {
            throw new ArgumentException(
                "Inspection query cannot be empty.",
                nameof(currentUserMessage)
            );
        }

        var agent =
            await _agentRepository.GetByIdAsync(agentId, cancellationToken)
            ?? throw new InvalidOperationException($"Agent '{agentId}' was not found.");

        Conversation conversation;
        UserProfile? userProfile = null;

        if (conversationId.HasValue)
        {
            conversation =
                await _conversationRepository.GetByIdWithMessagesAsync(
                    conversationId.Value,
                    cancellationToken
                )
                ?? throw new InvalidOperationException(
                    $"Conversation '{conversationId.Value}' was not found."
                );

            userProfile = conversation.UserProfile;
        }
        else
        {
            if (userProfileId.HasValue)
            {
                userProfile =
                    await _userProfileRepository.GetByIdAsync(
                        userProfileId.Value,
                        cancellationToken
                    )
                    ?? throw new InvalidOperationException(
                        $"User profile '{userProfileId.Value}' was not found."
                    );
            }

            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                AgentId = agent.Id,
                UserProfileId = userProfile?.Id,
                UserProfile = userProfile,
                Title = "Inspection Preview",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        ModelProfile? modelProfile = null;
        if (agent.DefaultModelProfileId.HasValue)
        {
            modelProfile = await _modelProfileRepository.GetByIdAsync(
                agent.DefaultModelProfileId.Value,
                cancellationToken
            );
        }

        GenerationPreset? generationPreset = null;
        if (agent.DefaultGenerationPresetId.HasValue)
        {
            generationPreset = await _generationPresetRepository.GetByIdAsync(
                agent.DefaultGenerationPresetId.Value,
                cancellationToken
            );
        }

        var contextInfo = await _modelContextService.GetForModelAsync(
            modelProfile?.ModelIdentifier,
            modelProfile?.ContextWindow,
            cancellationToken
        );

        var retrievalInspection = await _retrievalService.InspectAsync(
            agent.Id,
            conversationId,
            currentUserMessage,
            cancellationToken
        );

        var latestSummary = conversationId.HasValue
            ? await _conversationRepository.GetLatestSummaryAsync(
                conversation.Id,
                cancellationToken
            )
            : null;

        var orderedPriorMessages = conversation.Messages.OrderBy(x => x.SequenceNumber).ToList();

        var rawMessages = GetRawMessagesAfterSummary(orderedPriorMessages, latestSummary);

        var trimmedRawMessages = TrimRawHistoryToFit(
            agent,
            userProfile,
            conversation,
            retrievalInspection.SelectedMemories,
            retrievalInspection.SelectedLoreEntries,
            latestSummary?.SummaryText,
            rawMessages,
            currentUserMessage,
            contextInfo.MaxPromptTokens
        );

        var prompt = _promptComposer.Compose(
            new PromptCompositionContext
            {
                Agent = agent,
                UserProfile = userProfile,
                Conversation = conversation,
                PriorMessages = trimmedRawMessages,
                CurrentUserMessage = currentUserMessage,
                RollingSummary = latestSummary?.SummaryText,
                ExplicitMemories = retrievalInspection.SelectedMemories,
                RelevantLoreEntries = retrievalInspection.SelectedLoreEntries,
            }
        );

        var summaryUsedInPrompt = prompt.Sections.Any(x =>
            x.Name == "Rolling Summary" && !string.IsNullOrWhiteSpace(x.Content)
        );

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
            SelectedSessionState = prompt.SelectedSessionState,
            SuppressedSessionState = prompt.SuppressedSessionState,
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
            ExcludedRawMessageCount = excludedRawMessageCount,
        };
    }

    private IReadOnlyList<Message> TrimRawHistoryToFit(
        Agent agent,
        UserProfile? userProfile,
        Conversation conversation,
        IReadOnlyList<MemoryItem> explicitMemories,
        IReadOnlyList<LoreEntry> relevantLoreEntries,
        string? rollingSummary,
        IReadOnlyList<Message> rawMessages,
        string currentUserMessage,
        int maxPromptTokens
    )
    {
        var workingMessages = rawMessages.OrderBy(x => x.SequenceNumber).ToList();

        while (true)
        {
            var prompt = _promptComposer.Compose(
                new PromptCompositionContext
                {
                    Agent = agent,
                    UserProfile = userProfile,
                    Conversation = conversation,
                    PriorMessages = workingMessages,
                    CurrentUserMessage = currentUserMessage,
                    RollingSummary = rollingSummary,
                    ExplicitMemories = explicitMemories,
                    RelevantLoreEntries = relevantLoreEntries,
                }
            );

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
        SummaryCheckpoint? latestSummary
    )
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
