namespace LocalChat.Application.Background;

public sealed class ConversationBackgroundWorkOptions
{
    public bool Enabled { get; set; } = true;

    public int PollIntervalMilliseconds { get; set; } = 1000;

    public int RetrievalDebounceMilliseconds { get; set; } = 1500;

    public int MemoryDebounceMilliseconds { get; set; } = 4000;

    public int SummaryDebounceMilliseconds { get; set; } = 7000;

    public int SummaryMinMessagesBeforeRefresh { get; set; } = 10;

    public int SummaryRecentMessagesToKeepRaw { get; set; } = 6;

    public int SummaryMinNewMessagesSinceLastRefresh { get; set; } = 4;

    public int SummaryMaxMessagesInPrompt { get; set; } = 40;
}
