namespace LocalChat.Contracts.Inspection;

public sealed class SuppressedMemoryResponse
{
    public required Guid MemoryId { get; init; }

    public required string Category { get; init; }

    public required string Kind { get; init; }

    public required string Content { get; init; }

    public string? SlotKey { get; init; }

    public required double FinalScore { get; init; }

    public required string Reason { get; init; }
}