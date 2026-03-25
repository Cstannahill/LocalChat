namespace LocalChat.Contracts.Chat;

public sealed class RegenerateAssistantMessageRequest
{
    public required Guid ConversationId { get; init; }

    public required Guid AssistantMessageId { get; init; }
}
