using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class AppRuntimeDefaultsRepository : IAppRuntimeDefaultsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AppRuntimeDefaultsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppRuntimeDefaults?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppRuntimeDefaults.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AppRuntimeDefaults> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = new AppRuntimeDefaults
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.AppRuntimeDefaults.AddAsync(created, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return created;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
