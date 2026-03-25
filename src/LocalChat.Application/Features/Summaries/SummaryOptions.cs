namespace LocalChat.Application.Features.Summaries;

public sealed class SummaryOptions
{
    public const string SectionName = "Summaries";

    public bool Enabled { get; set; } = true;

    public int KeepRecentMessageCount { get; set; } = 12;

    public int MinMessagesToSummarize { get; set; } = 8;

    public int MaxMessagesPerPass { get; set; } = 20;

    public int MaxSummaryCharacters { get; set; } = 2000;
}
