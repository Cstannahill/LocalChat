using LocalChat.Domain.Entities.Characters;

namespace LocalChat.Application.Abstractions.Persistence;

public interface ICharacterRepository
{
    Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Character?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Character?> GetDefaultAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Character>> ListAsync(CancellationToken cancellationToken = default);

    Task<Character> AddAsync(Character character, CancellationToken cancellationToken = default);

    Task<bool> HasConversationsAsync(
        Guid characterId,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Remove(Character character);
}
