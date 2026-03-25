using LocalChat.Domain.Entities.Lorebooks;

namespace LocalChat.Application.Abstractions.Persistence;

public interface ILorebookRepository
{
    Task<IReadOnlyList<Lorebook>> ListLorebooksAsync(
        Guid? characterId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<LoreEntry>> ListRelevantEntriesAsync(
        Guid characterId,
        CancellationToken cancellationToken = default
    );

    Task<Lorebook?> GetLorebookByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LoreEntry?> GetLoreEntryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Lorebook> AddLorebookAsync(
        Lorebook lorebook,
        CancellationToken cancellationToken = default
    );

    Task<LoreEntry> AddLoreEntryAsync(
        LoreEntry loreEntry,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
