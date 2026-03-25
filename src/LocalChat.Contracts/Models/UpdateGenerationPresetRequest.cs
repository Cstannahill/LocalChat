namespace LocalChat.Contracts.Models;

public sealed class UpdateGenerationPresetRequest
{
    public required string Name { get; init; }

    public required double Temperature { get; init; }

    public required double TopP { get; init; }

    public required double RepeatPenalty { get; init; }

    public int? MaxOutputTokens { get; init; }

    public required string StopSequencesText { get; init; }

    public required string Notes { get; init; }
}
