namespace LocalChat.Contracts.Chat;

public sealed class GenerateSuggestedUserMessageRequest
{
    public required Guid ConversationId { get; init; }
}
