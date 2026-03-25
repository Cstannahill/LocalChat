namespace LocalChat.Application.Features.Memory;

public sealed class SceneStateCleanupResult
{
    public required int ScannedCount { get; init; }

    public required int RemovedCount { get; init; }

    public required IReadOnlyDictionary<string, int> RemovedByFamily { get; init; }
}
