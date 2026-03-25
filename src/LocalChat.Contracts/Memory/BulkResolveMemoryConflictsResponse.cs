namespace LocalChat.Contracts.Memory;

public sealed class BulkResolveMemoryConflictsResponse
{
    public required int ScannedConflictCount { get; init; }

    public required int MergedCount { get; init; }

    public required int SkippedCount { get; init; }

    public required IReadOnlyList<BulkResolvedMemoryConflictItemResponse> Items { get; init; }
}

public sealed class BulkResolvedMemoryConflictItemResponse
{
    public required Guid SourceMemoryId { get; init; }

    public required Guid TargetMemoryId { get; init; }

    public required string Strategy { get; init; }

    public required string Status { get; init; }

    public string? Message { get; init; }
}
