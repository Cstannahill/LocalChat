namespace LocalChat.Application.Abstractions.Retrieval;

public interface IVectorStore
{
    Task UpsertAsync(
        IReadOnlyList<VectorDocument> documents,
        CancellationToken cancellationToken = default
    );

    Task DeleteBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default
    );

    Task DeleteByConversationSourceTypeAsync(
        Guid conversationId,
        string sourceType,
        CancellationToken cancellationToken = default
    );

    Task DeleteByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    );

    Task DeleteAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchQuery query,
        CancellationToken cancellationToken = default
    );
}
