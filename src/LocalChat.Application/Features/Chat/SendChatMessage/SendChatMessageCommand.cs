namespace LocalChat.Application.Features.Chat.SendChatMessage;

public sealed class SendChatMessageCommand
{
    public required Guid CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? UserPersonaId { get; init; }

    public required string Message { get; init; }
}
