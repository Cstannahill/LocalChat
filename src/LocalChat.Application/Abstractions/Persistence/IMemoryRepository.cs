using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IMemoryRepository
{
    Task<IReadOnlyList<MemoryItem>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<MemoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryItem>> ListByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryItem>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryItem>> ListForProposalComparisonAsync(
        Guid characterId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<MemoryItem?> FindActiveByNormalizedKeyAsync(
        Guid characterId,
        Guid? conversationId,
        string normalizedKey,
        MemoryKind kind,
        CancellationToken cancellationToken = default);

    Task<MemoryItem?> FindTrackedBySlotAsync(
        Guid characterId,
        Guid? conversationId,
        string slotKey,
        MemoryKind kind,
        CancellationToken cancellationToken = default);

    Task<MemoryItem?> FindTrackedByFamilyAsync(
        Guid characterId,
        Guid? conversationId,
        MemorySlotFamily slotFamily,
        MemoryKind kind,
        CancellationToken cancellationToken = default);

    Task<int> DeleteExpiredSceneStateAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<int> DeleteByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<MemoryItem> AddAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
