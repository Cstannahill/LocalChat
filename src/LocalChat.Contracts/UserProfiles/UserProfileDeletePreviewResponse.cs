namespace LocalChat.Contracts.UserProfiles;

public sealed class UserProfileDeletePreviewResponse
{
    public required Guid UserProfileId { get; init; }

    public required string DisplayName { get; init; }

    public required bool IsDefault { get; init; }

    public required bool WillPromoteReplacement { get; init; }

    public Guid? ReplacementUserProfileId { get; init; }

    public string? ReplacementDisplayName { get; init; }
}
