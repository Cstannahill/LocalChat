namespace LocalChat.Contracts.Memory;

public sealed class MemoryItemResponse
{
    public required Guid Id { get; init; }

    public required string Category { get; init; }

    public string? Kind { get; init; }

    public string? ScopeType { get; init; }

    public required string Content { get; init; }

    public required string ReviewStatus { get; init; }

    public bool IsPinned { get; init; }

    public double? ConfidenceScore { get; init; }

    public string? ProposalReason { get; init; }

    public string? SourceExcerpt { get; init; }

    public string? NormalizedKey { get; init; }

    public string? SlotKey { get; init; }

    public string? SlotFamily { get; init; }

    public Guid? ConflictsWithMemoryItemId { get; init; }

    public int? SourceMessageSequenceNumber { get; init; }

    public int? LastObservedSequenceNumber { get; init; }

    public int? SupersededAtSequenceNumber { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }
}
