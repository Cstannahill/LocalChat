namespace LocalChat.Contracts.Admin;

public sealed class SessionStateCleanupResponse
{
    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public required int ScannedCount { get; init; }

    public required int RemovedCount { get; init; }

    public required IReadOnlyDictionary<string, int> RemovedByFamily { get; init; }
}