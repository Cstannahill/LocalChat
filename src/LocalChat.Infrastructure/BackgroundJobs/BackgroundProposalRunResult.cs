namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class BackgroundProposalRunResult
{
    public required bool Succeeded { get; init; }

    public Guid? ConversationId { get; init; }

    public required string Message { get; init; }

    public required int AttemptedCandidates { get; init; }

    public required int CreatedProposalCount { get; init; }

    public required int AutoSavedSessionStateCount { get; init; }

    public required int AutoAcceptedDurableCount { get; init; }

    public required int SkippedLowConfidenceCount { get; init; }

    public required int SkippedDuplicateCount { get; init; }

    public required int ConflictAnnotatedCount { get; init; }

    public required int InvalidCandidateCount { get; init; }
}
