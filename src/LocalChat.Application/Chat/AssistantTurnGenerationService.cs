using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Chat;

public sealed class AssistantTurnGenerationService : IAssistantTurnGenerationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IModelProfileRepository _modelProfileRepository;
    private readonly IGenerationPresetRepository _generationPresetRepository;
    private readonly IPromptComposer _promptComposer;
    private readonly IInferenceProvider _inferenceProvider;
    private readonly IRetrievalService _retrievalService;

    public AssistantTurnGenerationService(
        IConversationRepository conversationRepository,
        IModelProfileRepository modelProfileRepository,
        IGenerationPresetRepository generationPresetRepository,
        IPromptComposer promptComposer,
        IInferenceProvider inferenceProvider,
        IRetrievalService retrievalService)
    {
        _conversationRepository = conversationRepository;
        _modelProfileRepository = modelProfileRepository;
        _generationPresetRepository = generationPresetRepository;
        _promptComposer = promptComposer;
        _inferenceProvider = inferenceProvider;
        _retrievalService = retrievalService;
    }

    public async Task<GeneratedAssistantTurnResult> GenerateAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        if (orderedMessages.Count == 0)
        {
            throw new InvalidOperationException("Cannot generate an assistant reply for an empty conversation.");
        }

        var latestMessage = orderedMessages[^1];
        if (latestMessage.Role != MessageRole.User)
        {
            throw new InvalidOperationException("Assistant regeneration requires the latest remaining message to be a user message.");
        }

        var retrievalQuery = string.Join(
            "\n",
            orderedMessages
                .TakeLast(8)
                .Select(x => $"{x.Role}: {x.Content}"));

        var retrieval = await _retrievalService.InspectAsync(
            conversation.CharacterId,
            conversation.Id,
            retrievalQuery,
            cancellationToken);

        var rollingSummary = conversation.SummaryCheckpoints
            .OrderByDescending(x => x.EndSequenceNumber)
            .FirstOrDefault()?.SummaryText;

        var priorMessages = orderedMessages
            .Take(orderedMessages.Count - 1)
            .TakeLast(14)
            .ToList();

        var prompt = _promptComposer.Compose(new PromptCompositionContext
        {
            Character = conversation.Character
                        ?? throw new InvalidOperationException("Conversation character was not loaded."),
            Conversation = conversation,
            UserPersona = conversation.UserPersona,
            ExplicitMemories = retrieval.SelectedMemories,
            RelevantLoreEntries = retrieval.SelectedLoreEntries,
            RollingSummary = rollingSummary,
            PriorMessages = priorMessages,
            CurrentUserMessage = latestMessage.Content,
            ContinueWithoutUserMessage = false
        });

        var generationStartedAt = DateTime.UtcNow;
        var generatedText = await _inferenceProvider.StreamCompletionAsync(
            prompt.Prompt,
            static (_, _) => Task.CompletedTask,
            null,
            cancellationToken);
        var generationCompletedAt = DateTime.UtcNow;

        var finalText = generatedText.Trim();
        if (string.IsNullOrWhiteSpace(finalText))
        {
            throw new InvalidOperationException("Assistant regeneration returned empty content.");
        }

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

        var runtimeSelection = await ResolveRuntimeSelectionAsync(
            conversation.Character?.DefaultModelProfileId,
            conversation.Character?.DefaultGenerationPresetId,
            cancellationToken);

        var provenance = AssistantGenerationProvenance.Create(
            runtimeSelection.ModelProfile?.ProviderType,
            runtimeSelection.ModelProfile?.ModelIdentifier,
            runtimeSelection.ModelProfile?.Id,
            runtimeSelection.GenerationPreset?.Id);
        var variant = AssistantMessageVariantFactory.Create(
            finalText,
            0,
            provenance,
            generationStartedAt,
            generationCompletedAt);
        variant.MessageId = assistantMessage.Id;

        await _conversationRepository.AddMessageVariantAsync(variant, cancellationToken);

        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new GeneratedAssistantTurnResult
        {
            ConversationId = conversation.Id,
            AssistantMessageId = assistantMessage.Id,
            AssistantMessage = finalText
        };
    }

    private async Task<(ModelProfile? ModelProfile, GenerationPreset? GenerationPreset)> ResolveRuntimeSelectionAsync(
        Guid? modelProfileId,
        Guid? generationPresetId,
        CancellationToken cancellationToken)
    {
        var modelProfile = modelProfileId.HasValue
            ? await _modelProfileRepository.GetByIdAsync(modelProfileId.Value, cancellationToken)
            : null;

        var generationPreset = generationPresetId.HasValue
            ? await _generationPresetRepository.GetByIdAsync(generationPresetId.Value, cancellationToken)
            : null;

        return (modelProfile, generationPreset);
    }
}
