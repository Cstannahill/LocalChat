namespace LocalChat.Contracts.Admin;

public sealed class BackgroundProposalStatusResponse
{
    public required bool Enabled { get; init; }

    public required bool IsSweepRunning { get; init; }

    public DateTime? LastSweepStartedAt { get; init; }

    public DateTime? LastSweepCompletedAt { get; init; }

    public string? LastSweepMessage { get; init; }

    public int LastSweepTriggeredConversationCount { get; init; }

    public int CooldownTrackedConversationCount { get; init; }
}
