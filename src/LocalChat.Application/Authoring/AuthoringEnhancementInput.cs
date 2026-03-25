namespace LocalChat.Application.Authoring;

public sealed class AuthoringEnhancementInput
{
    public required string EntityType { get; init; }

    public required string FieldName { get; init; }

    public string CurrentText { get; init; } = string.Empty;

    public string Mode { get; init; } = "clarify";

    public string? ModelOverride { get; init; }

    public IReadOnlyDictionary<string, string> Context { get; init; } = new Dictionary<string, string>();
}
