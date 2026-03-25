namespace LocalChat.Contracts.Admin;

public sealed class MemoryRepairKeysResponse
{
    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public required int ScannedCount { get; init; }

    public required int RebuiltNormalizedKeyCount { get; init; }

    public required int RebuiltSlotKeyCount { get; init; }

    public required int RebuiltSlotFamilyCount { get; init; }

    public required int UpdatedCount { get; init; }
}
