using LocalChat.Application.Inspection;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Abstractions.Retrieval;

public interface IRetrievalService
{
    Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default);

    Task IndexLoreEntryAsync(
        Guid agentId,
        LoreEntry loreEntry,
        CancellationToken cancellationToken = default
    );

    Task RemoveSourceAsync(
        string sourceType,
        Guid sourceEntityId,
        CancellationToken cancellationToken = default
    );

    Task<RetrievalInspectionResult> InspectAsync(
        Guid agentId,
        Guid? conversationId,
        string query,
        CancellationToken cancellationToken = default
    );
}
