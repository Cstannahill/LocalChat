namespace LocalChat.Application.Inspection;

public sealed class MemoryExtractionAuditItem
{
    public required Guid EventId { get; init; }

    public required string Category { get; init; }

    public required string Kind { get; init; }

    public required string SlotFamily { get; init; }

    public string? SlotKey { get; init; }

    public required string CandidateContent { get; init; }

    public required string Action { get; init; }

    public required double ConfidenceScore { get; init; }

    public Guid? ExistingMemoryItemId { get; init; }

    public string? ExistingMemoryContent { get; init; }

    public string? Notes { get; init; }

    public required DateTime CreatedAt { get; init; }
}
