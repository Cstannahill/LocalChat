namespace LocalChat.Application.Features.Memory;

public interface ISceneStateCleanupService
{
    Task<SceneStateCleanupResult> PruneStaleAsync(CancellationToken cancellationToken = default);
}
