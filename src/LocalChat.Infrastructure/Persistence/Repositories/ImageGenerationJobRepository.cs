using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Images;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class ImageGenerationJobRepository : IImageGenerationJobRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ImageGenerationJobRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ImageGenerationJob> AddAsync(
        ImageGenerationJob job,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ImageGenerationJobs.AddAsync(job, cancellationToken);
        return job;
    }

    public async Task<ImageGenerationJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ImageGenerationJobs
            .Include(x => x.Assets.OrderBy(a => a.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ImageGenerationJob>> ListByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ImageGenerationJobs
            .AsNoTracking()
            .Include(x => x.Assets.OrderBy(a => a.SortOrder))
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
