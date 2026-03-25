using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Memory;

public sealed class MemoryAuditSnapshot
{
    public Guid Id { get; init; }

    public MemoryScopeType ScopeType { get; init; }

    public MemoryKind Kind { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? CharacterId { get; init; }

    public MemoryCategory Category { get; init; }

    public string Content { get; init; } = string.Empty;

    public string? NormalizedKey { get; init; }

    public string? SlotKey { get; init; }

    public MemorySlotFamily SlotFamily { get; init; }

    public int? SourceMessageSequenceNumber { get; init; }

    public int? LastObservedSequenceNumber { get; init; }

    public int? SupersededAtSequenceNumber { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public static MemoryAuditSnapshot From(MemoryItem item)
    {
        return new MemoryAuditSnapshot
        {
            Id = item.Id,
            ScopeType = item.ScopeType,
            Kind = item.Kind,
            ConversationId = item.ConversationId,
            CharacterId = item.CharacterId,
            Category = item.Category,
            Content = item.Content,
            NormalizedKey = item.NormalizedKey,
            SlotKey = item.SlotKey,
            SlotFamily = item.SlotFamily,
            SourceMessageSequenceNumber = item.SourceMessageSequenceNumber,
            LastObservedSequenceNumber = item.LastObservedSequenceNumber,
            SupersededAtSequenceNumber = item.SupersededAtSequenceNumber,
            ExpiresAt = item.ExpiresAt,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    public void ApplyTo(MemoryItem item)
    {
        item.ScopeType = ScopeType;
        item.Kind = Kind;
        item.ConversationId = ConversationId;
        item.CharacterId = CharacterId;
        item.Category = Category;
        item.Content = Content;
        item.NormalizedKey = NormalizedKey;
        item.SlotKey = SlotKey;
        item.SlotFamily = SlotFamily;
        item.SourceMessageSequenceNumber = SourceMessageSequenceNumber;
        item.LastObservedSequenceNumber = LastObservedSequenceNumber;
        item.SupersededAtSequenceNumber = SupersededAtSequenceNumber;
        item.ExpiresAt = ExpiresAt;
        item.CreatedAt = CreatedAt;
        item.UpdatedAt = UpdatedAt;
    }
}

public sealed class MemoryMergeAuditSnapshot
{
    public required MemoryAuditSnapshot Source { get; init; }

    public required MemoryAuditSnapshot Target { get; init; }
}
