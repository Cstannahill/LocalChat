using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);

    Task AddMessageAsync(Message message, CancellationToken cancellationToken = default);

    Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default);

    Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default);

    Task<int> DeleteMessagesFromSequenceAsync(
        Guid conversationId,
        int sequenceNumber,
        bool inclusive,
        CancellationToken cancellationToken = default);

    Task DeleteMessageVariantsAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<int> DeleteSummaryCheckpointsAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
