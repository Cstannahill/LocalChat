namespace LocalChat.Contracts.Conversations;

public sealed class UpdateConversationUserProfileRequest
{
    public Guid? UserProfileId { get; init; }
}
