namespace LocalChat.Application.Abstractions.Inference;

public interface IEmbeddingProvider
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
}
