namespace LocalChat.Application.Features.Memory;

public sealed class MemoryRepairKeysResult
{
    public required int ScannedCount { get; init; }

    public required int RebuiltNormalizedKeyCount { get; init; }

    public required int RebuiltSlotKeyCount { get; init; }

    public required int RebuiltSlotFamilyCount { get; init; }

    public required int UpdatedCount { get; init; }
}
