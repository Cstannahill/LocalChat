namespace LocalChat.Infrastructure.Options;

public sealed class BackgroundMemoryProposalOptions
{
    public bool Enabled { get; set; } = false;

    public int ScanIntervalSeconds { get; set; } = 90;

    public int RecentConversationWindowMinutes { get; set; } = 120;

    public int MinConversationMessageCount { get; set; } = 8;

    public int MinMinutesBetweenRunsPerConversation { get; set; } = 20;

    public bool SkipWhenPendingProposalsExist { get; set; } = true;

    public int MaxConversationsPerSweep { get; set; } = 10;
}
