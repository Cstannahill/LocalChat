namespace LocalChat.Contracts.Memory;

public sealed class MemoryConflictResponse
{
    public required Guid ProposalMemoryId { get; init; }

    public required string ProposalCategory { get; init; }

    public string? ProposalKind { get; init; }

    public required string ProposalContent { get; init; }

    public string? ProposalSlotKey { get; init; }

    public double? ProposalConfidenceScore { get; init; }

    public string? ProposalReason { get; init; }

    public string? ProposalSourceExcerpt { get; init; }

    public required Guid ConflictingMemoryId { get; init; }

    public required string ConflictingMemoryCategory { get; init; }

    public string? ConflictingMemoryKind { get; init; }

    public required string ConflictingMemoryContent { get; init; }

    public string? ConflictingMemorySlotKey { get; init; }

    public required string ConflictingMemoryReviewStatus { get; init; }

    public DateTime ProposalUpdatedAt { get; init; }

    public DateTime ConflictingMemoryUpdatedAt { get; init; }
}
