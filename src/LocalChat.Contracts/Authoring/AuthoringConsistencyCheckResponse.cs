namespace LocalChat.Contracts.Authoring;

public sealed class AuthoringConsistencyCheckResponse
{
    public required string EntityType { get; init; }

    public required string Summary { get; init; }

    public required IReadOnlyList<AuthoringConsistencyIssueResponse> Issues { get; init; }
}
