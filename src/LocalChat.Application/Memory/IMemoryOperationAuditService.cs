using LocalChat.Domain.Enums;

namespace LocalChat.Application.Memory;

public interface IMemoryOperationAuditService
{
    Task<Guid> RecordAsync(
        Guid memoryItemId,
        MemoryOperationType operationType,
        object? beforeState,
        object? afterState,
        Guid? sourceMemoryItemId = null,
        Guid? targetMemoryItemId = null,
        Guid? conversationId = null,
        Guid? characterId = null,
        int? messageSequenceNumber = null,
        string? note = null,
        CancellationToken cancellationToken = default);
}
