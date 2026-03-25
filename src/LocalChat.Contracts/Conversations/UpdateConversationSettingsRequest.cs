namespace LocalChat.Contracts.Conversations;

public sealed class UpdateConversationSettingsRequest
{
    public Guid? UserProfileId { get; init; }

    public Guid? RuntimeModelProfileOverrideId { get; init; }

    public Guid? RuntimeGenerationPresetOverrideId { get; init; }
}
