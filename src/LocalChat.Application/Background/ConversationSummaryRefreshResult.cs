namespace LocalChat.Application.Background;

public sealed class ConversationSummaryRefreshResult
{
    public required Guid ConversationId { get; init; }

    public bool Refreshed { get; init; }

    public string? Reason { get; init; }

    public int StartSequenceNumber { get; init; }

    public int EndSequenceNumber { get; init; }

    public string? SummaryText { get; init; }
}
