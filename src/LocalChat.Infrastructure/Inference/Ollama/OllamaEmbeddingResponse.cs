using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaEmbeddingResponse
{
    [JsonPropertyName("embeddings")]
    public List<List<float>> Embeddings { get; init; } = new();
}
