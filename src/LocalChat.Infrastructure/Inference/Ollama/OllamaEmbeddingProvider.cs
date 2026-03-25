using LocalChat.Application.Abstractions.Inference;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private readonly OllamaHttpClient _client;

    public OllamaEmbeddingProvider(OllamaHttpClient client)
    {
        _client = client;
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return _client.EmbedAsync(text, cancellationToken);
    }
}
