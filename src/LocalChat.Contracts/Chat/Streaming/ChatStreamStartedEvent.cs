namespace LocalChat.Contracts.Chat.Streaming;

public sealed class ChatStreamStartedEvent
{
    public required string Type { get; init; }

    public required Guid CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public required DateTime StartedAt { get; init; }
}