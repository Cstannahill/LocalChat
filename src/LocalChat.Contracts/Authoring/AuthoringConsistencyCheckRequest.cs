namespace LocalChat.Contracts.Authoring;

public sealed class AuthoringConsistencyCheckRequest
{
    public required string EntityType { get; init; }

    public IReadOnlyDictionary<string, string>? Fields { get; init; }

    public string? ModelOverride { get; init; }
}
