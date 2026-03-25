namespace LocalChat.Contracts.Tts;

public sealed class SynthesizeSpeechRequest
{
    public string? Voice { get; init; }

    public string? ModelIdentifier { get; init; }

    public double? Speed { get; init; }
}
