namespace LocalChat.Application.Abstractions.Retrieval;

public interface IRetrievalAdminService
{
    Task<RetrievalAdminStats> GetStatsAsync(CancellationToken cancellationToken = default);

    Task<RetrievalReindexResult> ReindexConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<RetrievalReindexResult> ReindexAllAsync(CancellationToken cancellationToken = default);
}
