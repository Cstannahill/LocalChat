namespace LocalChat.Contracts.Memory;

public sealed class BulkResolveMemoryConflictsRequest
{
    public Guid? ConversationId { get; init; }

    public Guid? CharacterId { get; init; }

    public int MaxCount { get; init; } = 50;

    public string Strategy { get; init; } = "append_unique";
}
