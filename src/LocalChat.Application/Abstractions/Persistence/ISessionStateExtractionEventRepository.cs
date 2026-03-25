using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Abstractions.Persistence;

public interface ISessionStateExtractionEventRepository
{
    Task AddAsync(SessionStateExtractionEvent item, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionStateExtractionEvent>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 100,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
