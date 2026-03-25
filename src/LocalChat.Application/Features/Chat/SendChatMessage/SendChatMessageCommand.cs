namespace LocalChat.Application.Features.Chat.SendChatMessage;

public sealed class SendChatMessageCommand
{
    public required Guid AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? UserProfileId { get; init; }

    public required string Message { get; init; }
}
