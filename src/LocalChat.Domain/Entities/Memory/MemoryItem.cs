using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Memory;

public sealed class MemoryItem
{
    public Guid Id { get; set; }

    public MemoryCategory Category { get; set; }

    public MemoryKind Kind { get; set; } = MemoryKind.DurableFact;

    public MemoryScopeType ScopeType { get; set; } = MemoryScopeType.Conversation;

    public Guid? AgentId { get; set; }

    public Guid? ConversationId { get; set; }

    public string Content { get; set; } = string.Empty;

    public MemoryReviewStatus ReviewStatus { get; set; } = MemoryReviewStatus.Accepted;

    public bool IsPinned { get; set; }

    public double? ConfidenceScore { get; set; }

    public string? ProposalReason { get; set; }

    public string? SourceExcerpt { get; set; }

    public string? NormalizedKey { get; set; }

    public string? SlotKey { get; set; }

    public MemorySlotFamily SlotFamily { get; set; } = MemorySlotFamily.None;

    public Guid? ConflictsWithMemoryItemId { get; set; }

    public int? SourceMessageSequenceNumber { get; set; }

    public int? LastObservedSequenceNumber { get; set; }

    public int? SupersededAtSequenceNumber { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
