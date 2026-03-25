namespace LocalChat.Contracts.Inspection;

public sealed class RetrievalInspectionRequest
{
    public required Guid AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public required string Query { get; init; }
}
