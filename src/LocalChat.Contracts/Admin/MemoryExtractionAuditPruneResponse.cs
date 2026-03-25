namespace LocalChat.Contracts.Admin;

public sealed class MemoryExtractionAuditPruneResponse
{
    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public required int OlderThanDays { get; init; }

    public required int DeletedCount { get; init; }
}