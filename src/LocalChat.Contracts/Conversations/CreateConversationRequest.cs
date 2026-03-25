namespace LocalChat.Contracts.Conversations;

public sealed class CreateConversationRequest
{
    public required Guid CharacterId { get; init; }

    public Guid? UserPersonaId { get; init; }

    public string? Title { get; init; }
}
