namespace LocalChat.Contracts.Authoring;

public sealed class AuthoringEnhancementRequest
{
    public required string EntityType { get; init; }

    public required string FieldName { get; init; }

    public string CurrentText { get; init; } = string.Empty;

    public string Mode { get; init; } = "clarify";

    public string? ModelOverride { get; init; }

    public IReadOnlyDictionary<string, string>? Context { get; init; }
}
