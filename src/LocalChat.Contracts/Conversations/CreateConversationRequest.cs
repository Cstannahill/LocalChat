namespace LocalChat.Contracts.Conversations;

public sealed class CreateConversationRequest
{
    public required Guid AgentId { get; init; }

    public Guid? UserProfileId { get; init; }

    public string? Title { get; init; }
}
