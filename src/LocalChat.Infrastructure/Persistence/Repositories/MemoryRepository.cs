using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class MemoryRepository : IMemoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MemoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MemoryItem>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .ToListAsync(cancellationToken);
    }

    public async Task<MemoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MemoryItem>> ListByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemoryItem>> ListByAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .AsNoTracking()
            .Where(x => x.AgentId == agentId)
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemoryItem>> ListForProposalComparisonAsync(
        Guid agentId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .Where(x =>
                x.AgentId == agentId &&
                (x.ConversationId == null || x.ConversationId == conversationId) &&
                x.ReviewStatus != MemoryReviewStatus.Rejected)
            .ToListAsync(cancellationToken);
    }

    public async Task<MemoryItem?> FindActiveByNormalizedKeyAsync(
        Guid agentId,
        Guid? conversationId,
        string normalizedKey,
        MemoryKind kind,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .FirstOrDefaultAsync(
                x =>
                    x.AgentId == agentId &&
                    x.ConversationId == conversationId &&
                    x.Kind == kind &&
                    x.NormalizedKey == normalizedKey &&
                    x.ReviewStatus == MemoryReviewStatus.Accepted &&
                    (kind != MemoryKind.SessionState || x.SupersededAtSequenceNumber == null),
                cancellationToken);
    }

    public async Task<MemoryItem?> FindTrackedBySlotAsync(
        Guid agentId,
        Guid? conversationId,
        string slotKey,
        MemoryKind kind,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .Where(x =>
                x.AgentId == agentId &&
                x.ConversationId == conversationId &&
                x.Kind == kind &&
                x.SlotKey == slotKey &&
                x.ReviewStatus != MemoryReviewStatus.Rejected &&
                (kind != MemoryKind.SessionState || x.SupersededAtSequenceNumber == null))
            .OrderByDescending(x => x.ReviewStatus == MemoryReviewStatus.Accepted)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MemoryItem?> FindTrackedByFamilyAsync(
        Guid agentId,
        Guid? conversationId,
        MemorySlotFamily slotFamily,
        MemoryKind kind,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemoryItems
            .Where(x =>
                x.AgentId == agentId &&
                x.ConversationId == conversationId &&
                x.Kind == kind &&
                x.SlotFamily == slotFamily &&
                x.ReviewStatus != MemoryReviewStatus.Rejected &&
                (kind != MemoryKind.SessionState || x.SupersededAtSequenceNumber == null))
            .OrderByDescending(x => x.ReviewStatus == MemoryReviewStatus.Accepted)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredSessionStateAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        _ = utcNow;

        var expired = await _dbContext.MemoryItems
            .Where(x =>
                x.Kind == MemoryKind.SessionState &&
                x.SupersededAtSequenceNumber != null)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return 0;
        }

        _dbContext.MemoryItems.RemoveRange(expired);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }

    public async Task<int> DeleteByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return 0;
        }

        var items = await _dbContext.MemoryItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return 0;
        }

        _dbContext.MemoryItems.RemoveRange(items);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return items.Count;
    }

    public async Task<MemoryItem> AddAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
    {
        await _dbContext.MemoryItems.AddAsync(memoryItem, cancellationToken);
        return memoryItem;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
