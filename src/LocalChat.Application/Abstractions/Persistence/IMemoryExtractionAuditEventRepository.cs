using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IMemoryExtractionAuditEventRepository
{
    Task AddAsync(MemoryExtractionAuditEvent item, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryExtractionAuditEvent>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 250,
        CancellationToken cancellationToken = default);

    Task<int> DeleteOlderThanAsync(
        DateTime utcCutoff,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
