using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Generation;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class GenerationPromptSnapshotRepository : IGenerationPromptSnapshotRepository
{
    private readonly ApplicationDbContext _dbContext;

    public GenerationPromptSnapshotRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(GenerationPromptSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _dbContext.GenerationPromptSnapshots.AddAsync(snapshot, cancellationToken);
    }

    public async Task<GenerationPromptSnapshot?> GetByMessageVariantIdAsync(
        Guid messageVariantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GenerationPromptSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MessageVariantId == messageVariantId, cancellationToken);
    }

    public async Task<IReadOnlyList<GenerationPromptSnapshot>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GenerationPromptSnapshots
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
