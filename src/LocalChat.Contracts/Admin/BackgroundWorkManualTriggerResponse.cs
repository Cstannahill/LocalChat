namespace LocalChat.Contracts.Admin;

public sealed class BackgroundWorkManualTriggerResponse
{
    public required Guid ConversationId { get; init; }

    public required string Operation { get; init; }

    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public bool? SummaryRefreshed { get; init; }

    public int? SummaryStartSequenceNumber { get; init; }

    public int? SummaryEndSequenceNumber { get; init; }

    public int? AttemptedCandidates { get; init; }

    public int? CreatedProposalCount { get; init; }

    public int? AutoSavedSceneStateCount { get; init; }

    public int? AutoAcceptedDurableCount { get; init; }

    public bool? RetrievalReindexed { get; init; }
}
