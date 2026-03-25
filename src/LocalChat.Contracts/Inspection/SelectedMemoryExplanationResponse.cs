namespace LocalChat.Contracts.Inspection;

public sealed class SelectedMemoryExplanationResponse
{
    public required Guid MemoryId { get; init; }

    public required string Category { get; init; }

    public required string Kind { get; init; }

    public required string Content { get; init; }

    public string? SlotKey { get; init; }

    public required double SemanticScore { get; init; }

    public required double FinalScore { get; init; }

    public required string WhySelected { get; init; }

    public required IReadOnlyList<SuppressedMemoryResponse> SuppressedMemories { get; init; }
}