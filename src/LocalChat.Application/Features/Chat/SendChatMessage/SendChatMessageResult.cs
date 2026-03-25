namespace LocalChat.Application.Features.Chat.SendChatMessage;

public sealed class SendChatMessageResult
{
    public required Guid ConversationId { get; init; }

    public required Guid UserMessageId { get; init; }

    public required Guid AssistantMessageId { get; init; }

    public required string AssistantMessage { get; init; }

    public required bool ConversationCreated { get; init; }
}