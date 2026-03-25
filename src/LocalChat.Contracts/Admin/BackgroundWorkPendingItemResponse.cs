namespace LocalChat.Contracts.Admin;

public sealed class BackgroundWorkPendingItemResponse
{
    public required Guid ConversationId { get; init; }

    public string? LastReason { get; init; }

    public required DateTime LastScheduledAt { get; init; }

    public DateTime? RetrievalDueAt { get; init; }

    public DateTime? MemoryDueAt { get; init; }

    public DateTime? SummaryDueAt { get; init; }

    public bool RetrievalDueNow { get; init; }

    public bool MemoryDueNow { get; init; }

    public bool SummaryDueNow { get; init; }
}
