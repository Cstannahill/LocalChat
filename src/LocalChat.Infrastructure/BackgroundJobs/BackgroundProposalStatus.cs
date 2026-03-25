namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class BackgroundProposalStatus
{
    public required bool Enabled { get; init; }

    public required bool IsSweepRunning { get; init; }

    public DateTime? LastSweepStartedAt { get; init; }

    public DateTime? LastSweepCompletedAt { get; init; }

    public string? LastSweepMessage { get; init; }

    public int LastSweepTriggeredConversationCount { get; init; }

    public int CooldownTrackedConversationCount { get; init; }
}
