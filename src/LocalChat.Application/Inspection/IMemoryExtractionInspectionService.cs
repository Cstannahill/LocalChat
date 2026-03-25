namespace LocalChat.Application.Inspection;

public interface IMemoryExtractionInspectionService
{
    Task<MemoryExtractionAuditResult> InspectConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
