using LocalChat.Domain.Entities.Settings;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IAppRuntimeDefaultsRepository
{
    Task<AppRuntimeDefaults?> GetAsync(CancellationToken cancellationToken = default);

    Task<AppRuntimeDefaults> GetOrCreateAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
