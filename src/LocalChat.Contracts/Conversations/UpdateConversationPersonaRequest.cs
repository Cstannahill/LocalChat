namespace LocalChat.Contracts.Conversations;

public sealed class UpdateConversationPersonaRequest
{
    public Guid? UserPersonaId { get; init; }
}
