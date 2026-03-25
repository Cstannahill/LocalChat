namespace LocalChat.Contracts.Agents;

public sealed class AgentResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Greeting { get; init; }

    public Guid? DefaultModelProfileId { get; init; }

    public Guid? DefaultGenerationPresetId { get; init; }

    public string? DefaultTtsVoice { get; init; }

    public string? DefaultVisualStylePreset { get; init; }

    public string? DefaultVisualPromptPrefix { get; init; }

    public string? DefaultVisualNegativePrompt { get; init; }

    public string? ImageUrl { get; init; }

    public DateTime? ImageUpdatedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
