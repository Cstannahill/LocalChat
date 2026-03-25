namespace LocalChat.Contracts.Memory;

public sealed class CreateMemoryItemRequest
{
    public required Guid AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public required string Category { get; init; }

    public required string Content { get; init; }

    public bool IsPinned { get; init; } = true;
}
