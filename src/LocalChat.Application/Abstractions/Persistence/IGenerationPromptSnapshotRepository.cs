using LocalChat.Domain.Entities.Generation;

namespace LocalChat.Application.Abstractions.Persistence;

public interface IGenerationPromptSnapshotRepository
{
    Task AddAsync(GenerationPromptSnapshot snapshot, CancellationToken cancellationToken = default);

    Task<GenerationPromptSnapshot?> GetByMessageVariantIdAsync(
        Guid messageVariantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GenerationPromptSnapshot>> ListByConversationAsync(
        Guid conversationId,
        int maxCount = 100,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
