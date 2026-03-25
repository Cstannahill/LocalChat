using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Memory;

public sealed class MemoryOperationAudit
{
    public Guid Id { get; set; }

    public Guid MemoryItemId { get; set; }

    public Guid? SourceMemoryItemId { get; set; }

    public Guid? TargetMemoryItemId { get; set; }

    public MemoryOperationType OperationType { get; set; }

    public Guid? ConversationId { get; set; }

    public Guid? AgentId { get; set; }

    public int? MessageSequenceNumber { get; set; }

    public string? BeforeStateJson { get; set; }

    public string? AfterStateJson { get; set; }

    public string? Note { get; set; }

    public bool IsUndone { get; set; }

    public DateTime? UndoneAtUtc { get; set; }

    public Guid? UndoAuditId { get; set; }

    public DateTime CreatedAt { get; set; }
}
