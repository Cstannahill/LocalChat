namespace LocalChat.Contracts.Chat;

public sealed class ContinueConversationRequest
{
    public required Guid ConversationId { get; init; }
}
