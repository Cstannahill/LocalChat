using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = true;

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; init; }

    [JsonPropertyName("options")]
    public OllamaGenerateOptions? Options { get; init; }
}

public sealed class OllamaGenerateOptions
{
    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; init; }

    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; init; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }

    [JsonPropertyName("repeat_penalty")]
    public double? RepeatPenalty { get; init; }

    [JsonPropertyName("stop")]
    public IReadOnlyList<string>? Stop { get; init; }
}
