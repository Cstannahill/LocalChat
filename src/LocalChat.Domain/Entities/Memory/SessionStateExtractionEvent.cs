using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Memory;

public sealed class SessionStateExtractionEvent
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid AgentId { get; set; }

    public MemorySlotFamily SlotFamily { get; set; } = MemorySlotFamily.None;

    public string? SlotKey { get; set; }

    public string CandidateContent { get; set; } = string.Empty;

    public string CandidateNormalizedKey { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }

    public string Action { get; set; } = string.Empty;

    public Guid? ReplacedMemoryItemId { get; set; }

    public string? ReplacedMemoryContent { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
