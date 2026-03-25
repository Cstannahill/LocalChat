namespace LocalChat.Contracts.Chat.Streaming;

public sealed class ChatCompletedEvent
{
    public required string Type { get; init; }

    public required Guid ConversationId { get; init; }

    public required Guid UserMessageId { get; init; }

    public required Guid AssistantMessageId { get; init; }

    public required string AssistantMessage { get; init; }

    public required bool ConversationCreated { get; init; }
}