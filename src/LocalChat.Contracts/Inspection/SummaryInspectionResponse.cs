namespace LocalChat.Contracts.Inspection;

public sealed class SummaryInspectionResponse
{
    public required Guid ConversationId { get; init; }

    public required bool HasSummaryCheckpoint { get; init; }

    public required bool SummaryUsedInPrompt { get; init; }

    public Guid? SummaryCheckpointId { get; init; }

    public DateTime? SummaryCreatedAt { get; init; }

    public int? StartSequenceNumber { get; init; }

    public int? EndSequenceNumber { get; init; }

    public required int SummaryCoveredMessageCount { get; init; }

    public required int TotalPriorMessageCount { get; init; }

    public required int IncludedRawMessageCount { get; init; }

    public required int ExcludedRawMessageCount { get; init; }

    public string? SummaryText { get; init; }
}
