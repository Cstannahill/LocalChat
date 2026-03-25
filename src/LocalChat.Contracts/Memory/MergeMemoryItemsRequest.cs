namespace LocalChat.Contracts.Memory;

public sealed class MergeMemoryItemsRequest
{
    public Guid? SourceMemoryId { get; init; }

    public Guid? TargetMemoryId { get; init; }

    public string Strategy { get; init; } = "append_unique";

    public bool PreferSourceContent { get; init; } = true;

    public string? SlotKeyOverride { get; init; }

    public string? SlotFamilyOverride { get; init; }

    public bool RejectSourceAfterMerge { get; init; } = true;

    public bool PreserveTargetReviewStatus { get; init; } = true;
}
