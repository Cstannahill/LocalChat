using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class SceneStateCleanupService : ISceneStateCleanupService
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly SceneStateCleanupOptions _options;

    public SceneStateCleanupService(
        IMemoryRepository memoryRepository,
        SceneStateCleanupOptions options)
    {
        _memoryRepository = memoryRepository;
        _options = options;
    }

    public async Task<SceneStateCleanupResult> PruneStaleAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new SceneStateCleanupResult
            {
                ScannedCount = 0,
                RemovedCount = 0,
                RemovedByFamily = new Dictionary<string, int>()
            };
        }

        var all = await _memoryRepository.ListAllAsync(cancellationToken);
        var scannedCount = all.Count;
        var stale = all
            .Where(x => x.Kind == MemoryKind.SceneState)
            .Where(x => !_options.PreservePinned || !x.IsPinned)
            .Where(x => x.SupersededAtSequenceNumber.HasValue)
            .ToList();

        var removedByFamily = stale
            .GroupBy(x => x.SlotFamily.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var removed = await _memoryRepository.DeleteByIdsAsync(
            stale.Select(x => x.Id).ToList(),
            cancellationToken);

        return new SceneStateCleanupResult
        {
            ScannedCount = scannedCount,
            RemovedCount = removed,
            RemovedByFamily = removedByFamily
        };
    }
}
