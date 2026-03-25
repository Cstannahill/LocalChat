using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaShowModelResponse
{
    [JsonPropertyName("parameters")]
    public string? Parameters { get; init; }

    [JsonPropertyName("model_info")]
    public Dictionary<string, JsonElement>? ModelInfo { get; init; }
}