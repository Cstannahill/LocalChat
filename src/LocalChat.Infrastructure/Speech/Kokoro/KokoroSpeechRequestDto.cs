using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Speech.Kokoro;

public sealed class KokoroSpeechRequestDto
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("voice")]
    public required string Voice { get; init; }

    [JsonPropertyName("input")]
    public required string Input { get; init; }

    [JsonPropertyName("response_format")]
    public required string ResponseFormat { get; init; }

    [JsonPropertyName("speed")]
    public double Speed { get; init; } = 1.0;
}
