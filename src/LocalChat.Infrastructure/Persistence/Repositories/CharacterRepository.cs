using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class CharacterRepository : ICharacterRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CharacterRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Character?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Characters.Include(x => x.SampleDialogues.OrderBy(d => d.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Character?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Characters.Include(x => x.SampleDialogues.OrderBy(d => d.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Character?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Characters.Include(x => x.SampleDialogues.OrderBy(d => d.SortOrder))
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Character>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Characters.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Character> AddAsync(
        Character character,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.Characters.AddAsync(character, cancellationToken);
        return character;
    }

    public async Task<bool> HasConversationsAsync(
        Guid characterId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Conversations.AnyAsync(
            x => x.CharacterId == characterId,
            cancellationToken
        );
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Character character)
    {
        _dbContext.Characters.Remove(character);
    }
}
