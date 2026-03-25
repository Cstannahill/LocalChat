namespace LocalChat.Contracts.Chat.Streaming;

public sealed class ChatTokenDeltaEvent
{
    public required string Type { get; init; }

    public required string Delta { get; init; }
}