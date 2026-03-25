namespace LocalChat.Contracts.Authoring;

public sealed class ConsistencyIssueRepairRequest
{
    public required string EntityType { get; init; }

    public required string FieldName { get; init; }

    public required string IssueType { get; init; }

    public required string IssueDescription { get; init; }

    public string? SuggestedFixHint { get; init; }

    public string CurrentText { get; init; } = string.Empty;

    public string? ModelOverride { get; init; }

    public IReadOnlyDictionary<string, string>? Context { get; init; }
}
