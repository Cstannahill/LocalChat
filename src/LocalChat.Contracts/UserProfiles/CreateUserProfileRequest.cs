namespace LocalChat.Contracts.UserProfiles;

public sealed class CreateUserProfileRequest
{
    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string Description { get; init; }

    public required string Traits { get; init; }

    public required string Preferences { get; init; }

    public required string AdditionalInstructions { get; init; }
}
