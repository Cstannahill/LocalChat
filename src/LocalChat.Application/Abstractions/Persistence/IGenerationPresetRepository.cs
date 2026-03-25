using LocalChat.Domain.Entities.Models;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IGenerationPresetRepository
{
    Task<GenerationPreset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GenerationPreset>> ListAsync(CancellationToken cancellationToken = default);

    Task<GenerationPreset> AddAsync(
        GenerationPreset generationPreset,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Remove(GenerationPreset generationPreset);
}
