namespace LocalChat.Application.Background;

[Flags]
public enum ConversationBackgroundWorkType
{
    None = 0,
    RetrievalReindex = 1 << 0,
    MemoryExtraction = 1 << 1,
    SummaryRefresh = 1 << 2,
    All = RetrievalReindex | MemoryExtraction | SummaryRefresh
}
