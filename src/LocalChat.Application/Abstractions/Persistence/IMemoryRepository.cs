using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IMemoryRepository
{
    Task<IReadOnlyList<MemoryItem>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<MemoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryItem>> ListByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryItem>> ListByAgentAsync(Guid agentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryItem>> ListForProposalComparisonAsync(
        Guid agentId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<MemoryItem?> FindActiveByNormalizedKeyAsync(
        Guid agentId,
        Guid? conversationId,
        string normalizedKey,
        MemoryKind kind,
        CancellationToken cancellationToken = default);

    Task<MemoryItem?> FindTrackedBySlotAsync(
        Guid agentId,
        Guid? conversationId,
        string slotKey,
        MemoryKind kind,
        CancellationToken cancellationToken = default);

    Task<MemoryItem?> FindTrackedByFamilyAsync(
        Guid agentId,
        Guid? conversationId,
        MemorySlotFamily slotFamily,
        MemoryKind kind,
        CancellationToken cancellationToken = default);

    Task<int> DeleteExpiredSessionStateAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<int> DeleteByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<MemoryItem> AddAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
