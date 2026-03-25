namespace LocalChat.Application.Abstractions.Speech;

public sealed class SpeechSynthesisResult
{
    public required byte[] AudioBytes { get; init; }

    public required string ContentType { get; init; }

    public required string ResponseFormat { get; init; }

    public required string EffectiveVoice { get; init; }

    public required string EffectiveModelIdentifier { get; init; }

    public required double EffectiveSpeed { get; init; }
}
