namespace LocalChat.Contracts.Memory;

public sealed class MemoryProvenanceResponse
{
    public required Guid Id { get; init; }

    public required string ScopeType { get; init; }

    public required string Kind { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? CharacterId { get; init; }

    public string? Category { get; init; }

    public required string Content { get; init; }

    public string? NormalizedKey { get; init; }

    public string? SlotKey { get; init; }

    public string? SceneFamily { get; init; }

    public int? SourceMessageSequenceNumber { get; init; }

    public int? LastObservedSequenceNumber { get; init; }

    public int? SupersededAtSequenceNumber { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required IReadOnlyList<MemoryOperationAuditEntryResponse> AuditEntries { get; init; }
}
