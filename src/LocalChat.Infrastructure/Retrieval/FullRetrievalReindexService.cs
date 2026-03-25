using LocalChat.Application.Abstractions.Retrieval;

namespace LocalChat.Infrastructure.Retrieval;

public sealed class FullRetrievalReindexService : IFullRetrievalReindexService
{
    private readonly VectorIndexingService _vectorIndexingService;

    public FullRetrievalReindexService(VectorIndexingService vectorIndexingService)
    {
        _vectorIndexingService = vectorIndexingService;
    }

    public Task<int> ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        return _vectorIndexingService.ReindexAllAsync(cancellationToken);
    }
}
