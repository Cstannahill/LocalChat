namespace LocalChat.Contracts.Memory;

public sealed class MergeMemoryItemsResponse
{
    public required Guid SourceMemoryId { get; init; }

    public required Guid TargetMemoryId { get; init; }

    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public string? TargetSlotKey { get; init; }

    public string? TargetReviewStatus { get; init; }

    public bool SourceRejected { get; init; }

    public bool RetrievalReindexed { get; init; }
}