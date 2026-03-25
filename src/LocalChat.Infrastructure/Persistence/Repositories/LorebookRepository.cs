using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Lorebooks;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class LorebookRepository : ILorebookRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LorebookRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Lorebook>> ListLorebooksAsync(
        Guid? characterId,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Lorebooks.AsNoTracking().Include(x => x.Entries).AsQueryable();

        if (characterId.HasValue)
        {
            query = query.Where(x => x.CharacterId == null || x.CharacterId == characterId.Value);
        }
        else
        {
            query = query.Where(x => x.CharacterId == null);
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoreEntry>> ListRelevantEntriesAsync(
        Guid characterId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .LoreEntries.AsNoTracking()
            .Include(x => x.Lorebook)
            .Where(x =>
                x.IsEnabled
                && x.Lorebook != null
                && (x.Lorebook.CharacterId == null || x.Lorebook.CharacterId == characterId)
            )
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lorebook?> GetLorebookByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Lorebooks.Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<LoreEntry?> GetLoreEntryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .LoreEntries.Include(x => x.Lorebook)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Lorebook> AddLorebookAsync(
        Lorebook lorebook,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.Lorebooks.AddAsync(lorebook, cancellationToken);
        return lorebook;
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
