using LocalChat.Domain.Entities.Images;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IImageGenerationJobRepository
{
    Task<ImageGenerationJob> AddAsync(ImageGenerationJob job, CancellationToken cancellationToken = default);

    Task<ImageGenerationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImageGenerationJob>> ListByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
