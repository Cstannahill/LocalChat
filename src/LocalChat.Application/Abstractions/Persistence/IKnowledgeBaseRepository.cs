using LocalChat.Domain.Entities.KnowledgeBases;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IKnowledgeBaseRepository
{
    Task<IReadOnlyList<KnowledgeBase>> ListKnowledgeBasesAsync(
        Guid? agentId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<LoreEntry>> ListRelevantEntriesAsync(
        Guid agentId,
        CancellationToken cancellationToken = default
    );

    Task<KnowledgeBase?> GetKnowledgeBaseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LoreEntry?> GetLoreEntryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<KnowledgeBase> AddKnowledgeBaseAsync(
        KnowledgeBase knowledgeBase,
        CancellationToken cancellationToken = default
    );

    Task<LoreEntry> AddLoreEntryAsync(
        LoreEntry loreEntry,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
