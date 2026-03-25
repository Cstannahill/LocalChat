namespace LocalChat.Contracts.Authoring;

public sealed class AuthoringConsistencyIssueResponse
{
    public required string Severity { get; init; }

    public required string FieldName { get; init; }

    public required string IssueType { get; init; }

    public required string Description { get; init; }

    public string? Suggestion { get; init; }
}
