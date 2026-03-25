using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Abstractions.Persistence;

public interface ISceneStateExtractionEventRepository
{
    Task AddAsync(SceneStateExtractionEvent item, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SceneStateExtractionEvent>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 100,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
