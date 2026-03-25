namespace LocalChat.Domain.Entities.Models;

public sealed class GenerationPreset
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Temperature { get; set; } = 0.8;

    public double TopP { get; set; } = 0.95;

    public double RepeatPenalty { get; set; } = 1.05;

    public int? MaxOutputTokens { get; set; }

    public string StopSequencesText { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
