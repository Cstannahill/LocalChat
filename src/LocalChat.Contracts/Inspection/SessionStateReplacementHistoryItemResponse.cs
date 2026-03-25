namespace LocalChat.Contracts.Inspection;

public sealed class SessionStateReplacementHistoryItemResponse
{
    public required Guid EventId { get; init; }

    public required string SlotFamily { get; init; }

    public string? SlotKey { get; init; }

    public required string CandidateContent { get; init; }

    public required string Action { get; init; }

    public Guid? ReplacedMemoryItemId { get; init; }

    public string? ReplacedMemoryContent { get; init; }

    public string? Notes { get; init; }

    public required DateTime CreatedAt { get; init; }
}
