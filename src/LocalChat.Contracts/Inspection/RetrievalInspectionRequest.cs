namespace LocalChat.Contracts.Inspection;

public sealed class RetrievalInspectionRequest
{
    public required Guid CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public required string Query { get; init; }
}
