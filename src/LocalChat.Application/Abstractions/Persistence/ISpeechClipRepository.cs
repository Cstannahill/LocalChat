using LocalChat.Domain.Entities.Audio;

namespace LocalChat.Application.Abstractions.Persistence;

public interface ISpeechClipRepository
{
    Task<SpeechClip> AddAsync(SpeechClip speechClip, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeechClip>> ListByMessageAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
