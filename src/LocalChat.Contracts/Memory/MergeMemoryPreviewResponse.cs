namespace LocalChat.Contracts.Memory;

public sealed class MergeMemoryPreviewResponse
{
    public required Guid SourceMemoryId { get; init; }

    public required Guid TargetMemoryId { get; init; }

    public required string Strategy { get; init; }

    public required string SourceContent { get; init; }

    public required string TargetContent { get; init; }

    public required string MergedContent { get; init; }

    public required bool ContentWillChange { get; init; }

    public string? ResultNormalizedKey { get; init; }

    public string? ResultCategory { get; init; }
}
