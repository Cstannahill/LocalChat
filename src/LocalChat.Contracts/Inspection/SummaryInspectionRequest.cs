namespace LocalChat.Contracts.Inspection;

public sealed class SummaryInspectionRequest
{
    public required Guid AgentId { get; init; }

    public required Guid ConversationId { get; init; }

    public string? Query { get; init; }
}
