using LocalChat.Application.Abstractions.Retrieval;

namespace LocalChat.Infrastructure.Retrieval;

public sealed class ConversationRetrievalSyncService : IConversationRetrievalSyncService
{
    private readonly VectorIndexingService _vectorIndexingService;

    public ConversationRetrievalSyncService(VectorIndexingService vectorIndexingService)
    {
        _vectorIndexingService = vectorIndexingService;
    }

    public async Task ReindexConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        await _vectorIndexingService.ReindexConversationAsync(conversationId, cancellationToken);
    }
}
