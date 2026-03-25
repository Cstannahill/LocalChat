namespace LocalChat.Application.Authoring;

public sealed class AuthoringStarterPack
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Concept { get; init; }

    public string? Vibe { get; init; }

    public string? Relationship { get; init; }

    public string? Setting { get; init; }
}
