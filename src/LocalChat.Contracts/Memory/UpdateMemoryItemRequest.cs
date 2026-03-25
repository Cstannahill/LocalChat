namespace LocalChat.Contracts.Memory;

public sealed class UpdateMemoryItemRequest
{
    public required string Content { get; init; }

    public string? Category { get; init; }

    public string? Kind { get; init; }

    public string? ReviewStatus { get; init; }

    public string? SlotKey { get; init; }

    public string? SlotFamily { get; init; }

    public bool? IsPinned { get; init; }

    public string? ProposalReason { get; init; }

    public string? SourceExcerpt { get; init; }

    public DateTime? ExpiresAt { get; init; }
}
