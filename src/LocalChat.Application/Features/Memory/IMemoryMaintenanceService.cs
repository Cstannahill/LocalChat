namespace LocalChat.Application.Features.Memory;

public interface IMemoryMaintenanceService
{
    Task<MemoryRepairKeysResult> RebuildKeysAsync(CancellationToken cancellationToken = default);
}
