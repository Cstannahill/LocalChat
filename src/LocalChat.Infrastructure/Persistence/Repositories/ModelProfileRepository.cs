using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class ModelProfileRepository : IModelProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ModelProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModelProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.ModelProfiles.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<ModelProfile>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .ModelProfiles.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ModelProfile> AddAsync(
        ModelProfile modelProfile,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.ModelProfiles.AddAsync(modelProfile, cancellationToken);
        return modelProfile;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(ModelProfile modelProfile)
    {
        _dbContext.ModelProfiles.Remove(modelProfile);
    }
}
