using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Agents;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class AgentRepository : IAgentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AgentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Agent?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Agents.Include(x => x.SampleDialogues.OrderBy(d => d.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Agent?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Agents.Include(x => x.SampleDialogues.OrderBy(d => d.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Agent?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Agents.Include(x => x.SampleDialogues.OrderBy(d => d.SortOrder))
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Agent>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Agents.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Agent> AddAsync(
        Agent agent,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.Agents.AddAsync(agent, cancellationToken);
        return agent;
    }

    public async Task<bool> HasConversationsAsync(
        Guid agentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Conversations.AnyAsync(
            x => x.AgentId == agentId,
            cancellationToken
        );
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Agent agent)
    {
        _dbContext.Agents.Remove(agent);
    }
}
