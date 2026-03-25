using LocalChat.Application.Inspection;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Abstractions.Retrieval;

public interface IRetrievalService
{
    Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default);

    Task IndexLoreEntryAsync(
        Guid characterId,
        LoreEntry loreEntry,
        CancellationToken cancellationToken = default
    );

    Task RemoveSourceAsync(
        string sourceType,
        Guid sourceEntityId,
        CancellationToken cancellationToken = default
    );

    Task<RetrievalInspectionResult> InspectAsync(
        Guid characterId,
        Guid? conversationId,
        string query,
        CancellationToken cancellationToken = default
    );
}
