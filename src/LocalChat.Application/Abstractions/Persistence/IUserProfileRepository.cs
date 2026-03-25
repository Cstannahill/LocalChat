using LocalChat.Domain.Entities.UserProfiles;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserProfile>> ListAsync(CancellationToken cancellationToken = default);

    Task<UserProfile> AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Remove(UserProfile userProfile);
}
