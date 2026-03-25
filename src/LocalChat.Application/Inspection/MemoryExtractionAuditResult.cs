namespace LocalChat.Application.Inspection;

public sealed class MemoryExtractionAuditResult
{
    public required Guid ConversationId { get; init; }

    public required int TotalEventCount { get; init; }

    public required int DurableEventCount { get; init; }

    public required int SessionStateEventCount { get; init; }

    public required IReadOnlyList<MemoryExtractionAuditItem> Events { get; init; }
}
