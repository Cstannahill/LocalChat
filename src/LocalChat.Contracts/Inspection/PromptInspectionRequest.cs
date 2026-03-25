namespace LocalChat.Contracts.Inspection;

public sealed class PromptInspectionRequest
{
    public required Guid CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? UserPersonaId { get; init; }

    public required string Query { get; init; }
}
