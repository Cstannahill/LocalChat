using LocalChat.Domain.Entities.Agents;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IAgentRepository
{
    Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Agent?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Agent?> GetDefaultAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Agent>> ListAsync(CancellationToken cancellationToken = default);

    Task<Agent> AddAsync(Agent agent, CancellationToken cancellationToken = default);

    Task<bool> HasConversationsAsync(
        Guid agentId,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    void Remove(Agent agent);
}
