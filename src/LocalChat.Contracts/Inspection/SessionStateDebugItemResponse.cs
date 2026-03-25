namespace LocalChat.Contracts.Inspection;

public sealed class SessionStateDebugItemResponse
{
    public required Guid MemoryId { get; init; }

    public required string Content { get; init; }

    public required string SlotFamily { get; init; }

    public string? SlotKey { get; init; }

    public required string ReviewStatus { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public required DateTime UpdatedAt { get; init; }
}
