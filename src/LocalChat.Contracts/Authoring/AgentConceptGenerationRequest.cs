namespace LocalChat.Contracts.Authoring;

public sealed class AgentConceptGenerationRequest
{
    public required string Concept { get; init; }

    public string? Vibe { get; init; }

    public string? Relationship { get; init; }

    public string? Setting { get; init; }

    public string? ModelOverride { get; init; }

    public IReadOnlyDictionary<string, string>? ExistingContext { get; init; }
}
