namespace LocalChat.Application.Abstractions.Retrieval;

public interface IConversationRetrievalSyncService
{
    Task ReindexConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
