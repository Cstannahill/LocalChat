namespace LocalChat.Contracts.Authoring;

public sealed class FullAuthoringBundleGenerationResponse
{
    public required string CharacterName { get; init; }

    public required string CharacterDescription { get; init; }

    public required string CharacterPersonalityDefinition { get; init; }

    public required string CharacterScenario { get; init; }

    public required string CharacterGreeting { get; init; }

    public required string PersonaDisplayName { get; init; }

    public required string PersonaDescription { get; init; }

    public required string PersonaTraits { get; init; }

    public required string PersonaPreferences { get; init; }

    public required string PersonaAdditionalInstructions { get; init; }

    public string? Rationale { get; init; }
}
