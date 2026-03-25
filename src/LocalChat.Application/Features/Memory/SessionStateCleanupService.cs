using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class SessionStateCleanupService : ISessionStateCleanupService
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly SessionStateCleanupOptions _options;

    public SessionStateCleanupService(
        IMemoryRepository memoryRepository,
        SessionStateCleanupOptions options)
    {
        _memoryRepository = memoryRepository;
        _options = options;
    }

    public async Task<SessionStateCleanupResult> PruneStaleAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new SessionStateCleanupResult
            {
                ScannedCount = 0,
                RemovedCount = 0,
                RemovedByFamily = new Dictionary<string, int>()
            };
        }

        var all = await _memoryRepository.ListAllAsync(cancellationToken);
        var scannedCount = all.Count;
        var stale = all
            .Where(x => x.Kind == MemoryKind.SessionState)
            .Where(x => !_options.PreservePinned || !x.IsPinned)
            .Where(x => x.SupersededAtSequenceNumber.HasValue)
            .ToList();

        var removedByFamily = stale
            .GroupBy(x => x.SlotFamily.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var removed = await _memoryRepository.DeleteByIdsAsync(
            stale.Select(x => x.Id).ToList(),
            cancellationToken);

        return new SessionStateCleanupResult
        {
            ScannedCount = scannedCount,
            RemovedCount = removed,
            RemovedByFamily = removedByFamily
        };
    }
}
