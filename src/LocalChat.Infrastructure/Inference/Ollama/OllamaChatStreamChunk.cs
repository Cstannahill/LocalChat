using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaChatStreamChunk
{
    [JsonPropertyName("response")]
    public string? Response { get; init; }

    [JsonPropertyName("done")]
    public bool Done { get; init; }
}