using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.UserProfiles;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.UserProfiles.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<UserProfile>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .UserProfiles.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserProfile> AddAsync(
        UserProfile userProfile,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.UserProfiles.AddAsync(userProfile, cancellationToken);
        return userProfile;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(UserProfile userProfile)
    {
        _dbContext.UserProfiles.Remove(userProfile);
    }
}
