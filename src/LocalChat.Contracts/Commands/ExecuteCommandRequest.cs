namespace LocalChat.Contracts.Commands;

public sealed class ExecuteCommandRequest
{
    public Guid? AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public required string CommandText { get; init; }
}
