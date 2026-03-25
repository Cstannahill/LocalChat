namespace LocalChat.Application.Authoring;

public sealed class CharacterConceptGenerationResult
{
    public required string Description { get; init; }

    public required string PersonalityDefinition { get; init; }

    public required string Scenario { get; init; }

    public required string Greeting { get; init; }

    public string? Rationale { get; init; }
}
