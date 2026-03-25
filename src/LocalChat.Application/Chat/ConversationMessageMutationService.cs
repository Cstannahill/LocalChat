using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Contracts.Conversations;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Chat;

public sealed class ConversationMessageMutationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAssistantTurnGenerationService _assistantTurnGenerationService;
    private readonly IConversationRetrievalSyncService _conversationRetrievalSyncService;
    private readonly IConversationBackgroundWorkScheduler _backgroundWorkScheduler;

    public ConversationMessageMutationService(
        IConversationRepository conversationRepository,
        IAssistantTurnGenerationService assistantTurnGenerationService,
        IConversationRetrievalSyncService conversationRetrievalSyncService,
        IConversationBackgroundWorkScheduler backgroundWorkScheduler)
    {
        _conversationRepository = conversationRepository;
        _assistantTurnGenerationService = assistantTurnGenerationService;
        _conversationRetrievalSyncService = conversationRetrievalSyncService;
        _backgroundWorkScheduler = backgroundWorkScheduler;
    }

    public async Task<ConversationMessageMutationResponse> EditAsync(
        Guid conversationId,
        Guid messageId,
        string newContent,
        bool regenerateAssistant,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newContent))
        {
            throw new InvalidOperationException("Edited message content cannot be empty.");
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        var target = orderedMessages.FirstOrDefault(x => x.Id == messageId)
                     ?? throw new InvalidOperationException($"Message '{messageId}' was not found in conversation '{conversationId}'.");

        var deletedCount = await _conversationRepository.DeleteMessagesFromSequenceAsync(
            conversationId,
            target.SequenceNumber,
            inclusive: false,
            cancellationToken);

        target.Content = newContent.Trim();

        if (target.Role == MessageRole.Assistant)
        {
            regenerateAssistant = false;

            await _conversationRepository.DeleteMessageVariantsAsync(target.Id, cancellationToken);

            target.SelectedVariantIndex = 0;

            await _conversationRepository.AddMessageVariantAsync(
                new MessageVariant
                {
                    Id = Guid.NewGuid(),
                    MessageId = target.Id,
                    VariantIndex = 0,
                    Content = target.Content,
                    CreatedAt = DateTime.UtcNow
                },
                cancellationToken);
        }

        var deletedSummaryCount = await _conversationRepository.DeleteSummaryCheckpointsAsync(
            conversationId,
            cancellationToken);

        conversation.Title = BuildConversationTitle(
            conversation.Agent?.Name ?? "Conversation",
            orderedMessages
                .Where(x => x.SequenceNumber <= target.SequenceNumber)
                .Select(x => x.Role == target.Role && x.Id == target.Id
                    ? CloneWithContent(x, target.Content)
                    : x)
                .ToList());

        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        GeneratedAssistantTurnResult? regenerated = null;

        if (regenerateAssistant)
        {
            var refreshed = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                            ?? throw new InvalidOperationException("Conversation disappeared after edit.");

            var latest = refreshed.Messages
                .OrderBy(x => x.SequenceNumber)
                .LastOrDefault();

            if (latest?.Role == MessageRole.User)
            {
                regenerated = await _assistantTurnGenerationService.GenerateAsync(conversationId, cancellationToken);
            }
        }

        await _conversationRetrievalSyncService.ReindexConversationAsync(conversationId, cancellationToken);
        await _backgroundWorkScheduler.ScheduleConversationChangeAsync(
            conversationId,
            ConversationBackgroundWorkType.MemoryExtraction | ConversationBackgroundWorkType.SummaryRefresh,
            "message-edit",
            cancellationToken);

        return new ConversationMessageMutationResponse
        {
            ConversationId = conversationId,
            TargetMessageId = messageId,
            Operation = "Edit",
            DeletedMessageCount = deletedCount,
            SummariesInvalidated = deletedSummaryCount > 0,
            RetrievalReindexed = true,
            AssistantRegenerated = regenerated is not null,
            RegeneratedAssistantMessageId = regenerated?.AssistantMessageId,
            RegeneratedAssistantMessage = regenerated?.AssistantMessage
        };
    }

    public async Task<ConversationMessageMutationResponse> DeleteAsync(
        Guid conversationId,
        Guid messageId,
        bool regenerateAssistant,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        var target = orderedMessages.FirstOrDefault(x => x.Id == messageId)
                     ?? throw new InvalidOperationException($"Message '{messageId}' was not found in conversation '{conversationId}'.");

        if (orderedMessages.Count == 1)
        {
            throw new InvalidOperationException("Cannot delete the only remaining message. Delete the conversation instead.");
        }

        var firstSequence = orderedMessages.Min(x => x.SequenceNumber);
        if (target.SequenceNumber == firstSequence)
        {
            throw new InvalidOperationException("Cannot delete the first message of a conversation through message mutation. Delete the conversation instead.");
        }

        var remainingMessages = orderedMessages
            .Where(x => x.SequenceNumber < target.SequenceNumber)
            .ToList();

        var deletedCount = await _conversationRepository.DeleteMessagesFromSequenceAsync(
            conversationId,
            target.SequenceNumber,
            inclusive: true,
            cancellationToken);

        var deletedSummaryCount = await _conversationRepository.DeleteSummaryCheckpointsAsync(
            conversationId,
            cancellationToken);

        conversation.Title = BuildConversationTitle(
            conversation.Agent?.Name ?? "Conversation",
            remainingMessages);

        conversation.UpdatedAt = DateTime.UtcNow;

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        GeneratedAssistantTurnResult? regenerated = null;

        if (regenerateAssistant)
        {
            var refreshed = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                            ?? throw new InvalidOperationException("Conversation disappeared after delete.");

            var latest = refreshed.Messages
                .OrderBy(x => x.SequenceNumber)
                .LastOrDefault();

            if (latest?.Role == MessageRole.User)
            {
                regenerated = await _assistantTurnGenerationService.GenerateAsync(conversationId, cancellationToken);
            }
        }

        await _conversationRetrievalSyncService.ReindexConversationAsync(conversationId, cancellationToken);
        await _backgroundWorkScheduler.ScheduleConversationChangeAsync(
            conversationId,
            ConversationBackgroundWorkType.MemoryExtraction | ConversationBackgroundWorkType.SummaryRefresh,
            "message-delete",
            cancellationToken);

        return new ConversationMessageMutationResponse
        {
            ConversationId = conversationId,
            TargetMessageId = messageId,
            Operation = "Delete",
            DeletedMessageCount = deletedCount,
            SummariesInvalidated = deletedSummaryCount > 0,
            RetrievalReindexed = true,
            AssistantRegenerated = regenerated is not null,
            RegeneratedAssistantMessageId = regenerated?.AssistantMessageId,
            RegeneratedAssistantMessage = regenerated?.AssistantMessage
        };
    }

    private static string BuildConversationTitle(
        string agentName,
        IReadOnlyList<Message> messages)
    {
        var firstUserMessage = messages
            .Where(x => x.Role == MessageRole.User)
            .OrderBy(x => x.SequenceNumber)
            .FirstOrDefault();

        if (firstUserMessage is null || string.IsNullOrWhiteSpace(firstUserMessage.Content))
        {
            return $"{agentName}: New conversation";
        }

        var content = firstUserMessage.Content.Trim();
        var preview = content.Length <= 48 ? content : $"{content[..48]}...";

        return $"{agentName}: {preview}";
    }

    private static Message CloneWithContent(Message message, string newContent)
    {
        return new Message
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            Role = message.Role,
            OriginType = message.OriginType,
            Content = newContent,
            SequenceNumber = message.SequenceNumber,
            CreatedAt = message.CreatedAt,
            SelectedVariantIndex = message.SelectedVariantIndex
        };
    }
}
