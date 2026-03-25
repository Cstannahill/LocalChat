using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Personas;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class UserPersonaRepository : IUserPersonaRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserPersonaRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserPersona?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.UserPersonas.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<UserPersona>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .UserPersonas.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserPersona> AddAsync(
        UserPersona persona,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.UserPersonas.AddAsync(persona, cancellationToken);
        return persona;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(UserPersona persona)
    {
        _dbContext.UserPersonas.Remove(persona);
    }
}
