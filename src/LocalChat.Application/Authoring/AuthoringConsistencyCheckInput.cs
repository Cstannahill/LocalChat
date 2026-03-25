namespace LocalChat.Application.Authoring;

public sealed class AuthoringConsistencyCheckInput
{
    public required string EntityType { get; init; }

    public IReadOnlyDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>();

    public string? ModelOverride { get; init; }
}
