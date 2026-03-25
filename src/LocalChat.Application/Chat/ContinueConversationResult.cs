namespace LocalChat.Application.Chat;

public sealed class ContinueConversationResult
{
    public required Guid ConversationId { get; init; }

    public required Guid AssistantMessageId { get; init; }

    public required string AssistantMessage { get; init; }
}
