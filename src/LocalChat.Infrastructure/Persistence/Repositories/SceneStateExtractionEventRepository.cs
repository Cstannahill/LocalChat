using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Memory;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class SceneStateExtractionEventRepository : ISceneStateExtractionEventRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SceneStateExtractionEventRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(SceneStateExtractionEvent item, CancellationToken cancellationToken = default)
    {
        await _dbContext.SceneStateExtractionEvents.AddAsync(item, cancellationToken);
    }

    public async Task<IReadOnlyList<SceneStateExtractionEvent>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SceneStateExtractionEvents
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
