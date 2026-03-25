namespace LocalChat.Contracts.Chat;

public sealed class SendChatMessageRequest
{
    public required Guid CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? UserPersonaId { get; init; }

    public required string Message { get; init; }
}
