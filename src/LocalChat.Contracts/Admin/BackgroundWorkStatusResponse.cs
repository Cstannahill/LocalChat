namespace LocalChat.Contracts.Admin;

public sealed class BackgroundWorkStatusResponse
{
    public required bool QueueEnabled { get; init; }

    public required int PendingConversationCount { get; init; }

    public required IReadOnlyList<BackgroundWorkPendingItemResponse> PendingItems { get; init; }

    public bool BackgroundProposalSweepEnabled { get; init; }

    public bool BackgroundProposalSweepRunning { get; init; }

    public DateTime? LastSweepStartedAt { get; init; }

    public DateTime? LastSweepCompletedAt { get; init; }

    public string? LastSweepMessage { get; init; }

    public int LastSweepTriggeredConversationCount { get; init; }
}
