namespace LocalChat.Application.Features.Memory;

public interface ISessionStateCleanupService
{
    Task<SessionStateCleanupResult> PruneStaleAsync(CancellationToken cancellationToken = default);
}
