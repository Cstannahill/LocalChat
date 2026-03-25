namespace LocalChat.Contracts.Authoring;

public sealed class AuthoringEnhancementResponse
{
    public required string EntityType { get; init; }

    public required string FieldName { get; init; }

    public required string Mode { get; init; }

    public required string OriginalText { get; init; }

    public required string SuggestedText { get; init; }

    public string? Rationale { get; init; }
}
