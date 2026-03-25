using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("input")]
    public required string Input { get; init; }
}
