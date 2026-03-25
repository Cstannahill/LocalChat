using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Memory;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class SessionStateExtractionEventRepository : ISessionStateExtractionEventRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SessionStateExtractionEventRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(SessionStateExtractionEvent item, CancellationToken cancellationToken = default)
    {
        await _dbContext.SessionStateExtractionEvents.AddAsync(item, cancellationToken);
    }

    public async Task<IReadOnlyList<SessionStateExtractionEvent>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SessionStateExtractionEvents
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
