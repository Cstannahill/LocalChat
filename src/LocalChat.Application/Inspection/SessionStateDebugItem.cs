namespace LocalChat.Application.Inspection;

public sealed class SessionStateDebugItem
{
    public required Guid MemoryId { get; init; }

    public required string Content { get; init; }

    public required string SlotFamily { get; init; }

    public string? SlotKey { get; init; }

    public required string ReviewStatus { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
