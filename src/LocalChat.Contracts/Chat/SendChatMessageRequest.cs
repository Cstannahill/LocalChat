namespace LocalChat.Contracts.Chat;

public sealed class SendChatMessageRequest
{
    public required Guid AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? UserProfileId { get; init; }

    public required string Message { get; init; }
}
