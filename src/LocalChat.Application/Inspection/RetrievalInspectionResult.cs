using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Inspection;

public sealed class RetrievalInspectionResult
{
    public required string Query { get; init; }

    public required IReadOnlyList<MemoryItem> SelectedMemories { get; init; }

    public required IReadOnlyList<LoreEntry> SelectedLoreEntries { get; init; }

    public required IReadOnlyList<SelectedMemoryExplanation> SelectedMemoryExplanations { get; init; }

    public required IReadOnlyList<SelectedLoreExplanation> SelectedLoreExplanations { get; init; }
}