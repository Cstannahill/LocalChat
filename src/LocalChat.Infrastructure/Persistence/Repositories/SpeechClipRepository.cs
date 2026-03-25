using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Audio;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class SpeechClipRepository : ISpeechClipRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SpeechClipRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpeechClip> AddAsync(
        SpeechClip speechClip,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SpeechClips.AddAsync(speechClip, cancellationToken);
        return speechClip;
    }

    public async Task<IReadOnlyList<SpeechClip>> ListByMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SpeechClips
            .AsNoTracking()
            .Where(x => x.MessageId == messageId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
