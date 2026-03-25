namespace LocalChat.Application.Abstractions.Inference;

public sealed class InferenceExecutionSettings
{
    public string? ModelIdentifier { get; init; }

    public int? ContextWindow { get; init; }

    public int? MaxOutputTokens { get; init; }

    public double? Temperature { get; init; }

    public double? TopP { get; init; }

    public double? RepeatPenalty { get; init; }

    public IReadOnlyList<string> StopSequences { get; init; } = Array.Empty<string>();
}
