using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Memory;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class MemoryExtractionAuditEventRepository : IMemoryExtractionAuditEventRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MemoryExtractionAuditEventRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MemoryExtractionAuditEvent item, CancellationToken cancellationToken = default)
    {
        await _dbContext.MemoryExtractionAuditEvents.AddAsync(item, cancellationToken);
    }

    public async Task<IReadOnlyList<MemoryExtractionAuditEvent>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 250,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryExtractionAuditEvents
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(
        DateTime utcCutoff,
        CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.MemoryExtractionAuditEvents
            .Where(x => x.CreatedAt < utcCutoff)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return 0;
        }

        _dbContext.MemoryExtractionAuditEvents.RemoveRange(items);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return items.Count;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
