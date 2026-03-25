using LocalChat.Domain.Entities.Personas;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IUserPersonaRepository
{
    Task<UserPersona?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserPersona>> ListAsync(CancellationToken cancellationToken = default);

    Task<UserPersona> AddAsync(UserPersona persona, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Remove(UserPersona persona);
}
