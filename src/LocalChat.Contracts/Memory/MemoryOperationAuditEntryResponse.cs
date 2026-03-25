namespace LocalChat.Contracts.Memory;

public sealed class MemoryOperationAuditEntryResponse
{
    public required Guid Id { get; init; }

    public required string OperationType { get; init; }

    public int? MessageSequenceNumber { get; init; }

    public string? Note { get; init; }

    public required bool IsUndone { get; init; }

    public required bool CanUndo { get; init; }

    public required DateTime CreatedAt { get; init; }

    public string? BeforeContentPreview { get; init; }

    public string? AfterContentPreview { get; init; }
}
