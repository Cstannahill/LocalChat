using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.KnowledgeBases;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public KnowledgeBaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<KnowledgeBase>> ListKnowledgeBasesAsync(
        Guid? agentId,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.KnowledgeBases.AsNoTracking().Include(x => x.Entries).AsQueryable();

        if (agentId.HasValue)
        {
            query = query.Where(x => x.AgentId == null || x.AgentId == agentId.Value);
        }
        else
        {
            query = query.Where(x => x.AgentId == null);
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoreEntry>> ListRelevantEntriesAsync(
        Guid agentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .LoreEntries.AsNoTracking()
            .Include(x => x.KnowledgeBase)
            .Where(x =>
                x.IsEnabled
                && x.KnowledgeBase != null
                && (x.KnowledgeBase.AgentId == null || x.KnowledgeBase.AgentId == agentId)
            )
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<KnowledgeBase?> GetKnowledgeBaseByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .KnowledgeBases.Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<LoreEntry?> GetLoreEntryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .LoreEntries.Include(x => x.KnowledgeBase)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<KnowledgeBase> AddKnowledgeBaseAsync(
        KnowledgeBase knowledgeBase,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.KnowledgeBases.AddAsync(knowledgeBase, cancellationToken);
        return knowledgeBase;
    }

    public async Task<LoreEntry> AddLoreEntryAsync(
        LoreEntry loreEntry,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.LoreEntries.AddAsync(loreEntry, cancellationToken);
        return loreEntry;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
