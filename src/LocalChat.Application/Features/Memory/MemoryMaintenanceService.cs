using LocalChat.Application.Abstractions.Persistence;

namespace LocalChat.Application.Features.Memory;

public sealed class MemoryMaintenanceService : IMemoryMaintenanceService
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly MemoryProposalQualityEvaluator _qualityEvaluator;

    public MemoryMaintenanceService(
        IMemoryRepository memoryRepository,
        MemoryProposalQualityEvaluator qualityEvaluator)
    {
        _memoryRepository = memoryRepository;
        _qualityEvaluator = qualityEvaluator;
    }

    public async Task<MemoryRepairKeysResult> RebuildKeysAsync(CancellationToken cancellationToken = default)
    {
        var memories = await _memoryRepository.ListAllAsync(cancellationToken);

        var rebuiltNormalizedKeyCount = 0;
        var rebuiltSlotKeyCount = 0;
        var rebuiltSlotFamilyCount = 0;
        var updatedCount = 0;

        foreach (var memory in memories)
        {
            var newNormalizedKey = _qualityEvaluator.NormalizeKey(memory.Category, memory.Content);
            var newSlotKey = _qualityEvaluator.BuildSlotKey(memory.Category, memory.Content, memory.SlotKey);
            var newSlotFamily = _qualityEvaluator.BuildSlotFamily(memory.Category, memory.Content, memory.SlotKey, null);

            var changed = false;

            if (!string.Equals(memory.NormalizedKey, newNormalizedKey, StringComparison.Ordinal))
            {
                memory.NormalizedKey = newNormalizedKey;
                rebuiltNormalizedKeyCount++;
                changed = true;
            }

            if (!string.Equals(memory.SlotKey, newSlotKey, StringComparison.Ordinal))
            {
                memory.SlotKey = newSlotKey;
                rebuiltSlotKeyCount++;
                changed = true;
            }

            if (memory.SlotFamily != newSlotFamily)
            {
                memory.SlotFamily = newSlotFamily;
                rebuiltSlotFamilyCount++;
                changed = true;
            }

            if (changed)
            {
                memory.UpdatedAt = DateTime.UtcNow;
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            await _memoryRepository.SaveChangesAsync(cancellationToken);
        }

        return new MemoryRepairKeysResult
        {
            ScannedCount = memories.Count,
            RebuiltNormalizedKeyCount = rebuiltNormalizedKeyCount,
            RebuiltSlotKeyCount = rebuiltSlotKeyCount,
            RebuiltSlotFamilyCount = rebuiltSlotFamilyCount,
            UpdatedCount = updatedCount
        };
    }
}
