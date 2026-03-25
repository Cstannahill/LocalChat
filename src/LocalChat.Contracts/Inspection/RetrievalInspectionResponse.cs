using LocalChat.Contracts.Lorebooks;
using LocalChat.Contracts.Memory;

namespace LocalChat.Contracts.Inspection;

public sealed class RetrievalInspectionResponse
{
    public required string Query { get; init; }

    public required IReadOnlyList<MemoryItemResponse> SelectedMemories { get; init; }

    public required IReadOnlyList<LoreEntryResponse> SelectedLoreEntries { get; init; }

    public required IReadOnlyList<SelectedMemoryExplanationResponse> SelectedMemoryExplanations { get; init; }

    public required IReadOnlyList<SelectedLoreExplanationResponse> SelectedLoreExplanations { get; init; }
}