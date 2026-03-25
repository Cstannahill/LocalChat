namespace LocalChat.Application.Features.Memory;

public sealed class MemoryProposalGenerationResult
{
    public required int AttemptedCandidates { get; init; }

    public required int CreatedProposalCount { get; init; }

    public required int AutoSavedSessionStateCount { get; init; }

    public required int AutoAcceptedDurableCount { get; init; }

    public required int SessionStateReplacedCount { get; init; }

    public required int MergedDurableProposalCount { get; init; }

    public required int ConflictingDurableProposalCount { get; init; }

    public required int SkippedLowConfidenceCount { get; init; }

    public required int SkippedDuplicateCount { get; init; }

    public required int ConflictAnnotatedCount { get; init; }

    public required int InvalidCandidateCount { get; init; }
}