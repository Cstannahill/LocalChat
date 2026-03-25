using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class GenerationPresetRepository : IGenerationPresetRepository
{
    private readonly ApplicationDbContext _dbContext;

    public GenerationPresetRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GenerationPreset?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.GenerationPresets.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<GenerationPreset>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .GenerationPresets.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<GenerationPreset> AddAsync(
        GenerationPreset generationPreset,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.GenerationPresets.AddAsync(generationPreset, cancellationToken);
        return generationPreset;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(GenerationPreset generationPreset)
    {
        _dbContext.GenerationPresets.Remove(generationPreset);
    }
}
