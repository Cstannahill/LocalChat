namespace LocalChat.Application.Authoring;

public sealed class AuthoringConsistencyCheckResult
{
    public required string EntityType { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyList<AuthoringConsistencyIssue> Issues { get; init; }
}
