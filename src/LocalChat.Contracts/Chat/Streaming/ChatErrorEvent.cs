namespace LocalChat.Contracts.Chat.Streaming;

public sealed class ChatErrorEvent
{
    public required string Type { get; init; }

    public required string Message { get; init; }
}