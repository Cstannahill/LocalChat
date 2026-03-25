namespace LocalChat.Application.Authoring;

public sealed class FullAuthoringBundleGenerationResult
{
    public required string AgentName { get; init; }

    public required string AgentDescription { get; init; }

    public required string AgentPersonalityDefinition { get; init; }

    public required string AgentScenario { get; init; }

    public required string AgentGreeting { get; init; }

    public required string UserProfileDisplayName { get; init; }

    public required string UserProfileDescription { get; init; }

    public required string UserProfileTraits { get; init; }

    public required string UserProfilePreferences { get; init; }

    public required string UserProfileAdditionalInstructions { get; init; }

    public string? Rationale { get; init; }
}
