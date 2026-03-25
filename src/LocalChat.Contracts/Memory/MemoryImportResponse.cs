namespace LocalChat.Contracts.Memory;

public sealed class MemoryImportResponse
{
    public required string Format { get; init; }

    public required string Strategy { get; init; }

    public required int ImportedCount { get; init; }

    public required int UpdatedCount { get; init; }

    public required int SkippedCount { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
