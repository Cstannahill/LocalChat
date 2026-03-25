namespace LocalChat.Application.Abstractions.Retrieval;

public interface IFullRetrievalReindexService
{
    Task<int> ReindexAllAsync(CancellationToken cancellationToken = default);
}
