namespace LocalChat.Application.Authoring;

public sealed class FullAuthoringBundleGenerationInput
{
    public required string Concept { get; init; }

    public string? Vibe { get; init; }

    public string? Relationship { get; init; }

    public string? Setting { get; init; }

    public string? ModelOverride { get; init; }

    public IReadOnlyDictionary<string, string> ExistingContext { get; init; } = new Dictionary<string, string>();
}
