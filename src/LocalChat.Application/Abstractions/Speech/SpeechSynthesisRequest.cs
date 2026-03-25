namespace LocalChat.Application.Abstractions.Speech;

public sealed class SpeechSynthesisRequest
{
    public required string Input { get; init; }

    public string? Voice { get; init; }

    public string? ModelIdentifier { get; init; }

    public string? ResponseFormat { get; init; }

    public double? Speed { get; init; }
}
