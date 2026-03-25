using System.Text.Json;
using LocalChat.Application.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class MemoryOperationAuditService : IMemoryOperationAuditService
{
    private readonly ApplicationDbContext _dbContext;

    public MemoryOperationAuditService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> RecordAsync(
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
        CancellationToken cancellationToken = default)
    {
        var audit = new MemoryOperationAudit
        {
            Id = Guid.NewGuid(),
            MemoryItemId = memoryItemId,
            SourceMemoryItemId = sourceMemoryItemId,
            TargetMemoryItemId = targetMemoryItemId,
            OperationType = operationType,
            ConversationId = conversationId,
            CharacterId = characterId,
            MessageSequenceNumber = messageSequenceNumber,
            BeforeStateJson = beforeState is null ? null : JsonSerializer.Serialize(beforeState),
            AfterStateJson = afterState is null ? null : JsonSerializer.Serialize(afterState),
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.MemoryOperationAudits.AddAsync(audit, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }
}
