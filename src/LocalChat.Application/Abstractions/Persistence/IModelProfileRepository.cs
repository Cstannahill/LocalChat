using LocalChat.Domain.Entities.Models;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IModelProfileRepository
{
    Task<ModelProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModelProfile>> ListAsync(CancellationToken cancellationToken = default);

    Task<ModelProfile> AddAsync(
        ModelProfile modelProfile,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Remove(ModelProfile modelProfile);
}
