using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Inspection;

public sealed class MemoryExtractionInspectionService : IMemoryExtractionInspectionService
{
    private readonly IMemoryExtractionAuditEventRepository _auditRepository;

    public MemoryExtractionInspectionService(IMemoryExtractionAuditEventRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<MemoryExtractionAuditResult> InspectConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditRepository.ListByConversationAsync(conversationId, 250, cancellationToken);

        return new MemoryExtractionAuditResult
        {
            ConversationId = conversationId,
            TotalEventCount = events.Count,
            DurableEventCount = events.Count(x => x.Kind == MemoryKind.DurableFact),
            SceneStateEventCount = events.Count(x => x.Kind == MemoryKind.SceneState),
            Events = events.Select(x => new MemoryExtractionAuditItem
            {
                EventId = x.Id,
                Category = x.Category.ToString(),
                Kind = x.Kind.ToString(),
                SlotFamily = x.SlotFamily.ToString(),
                SlotKey = x.SlotKey,
                CandidateContent = x.CandidateContent,
                Action = x.Action,
                ConfidenceScore = x.ConfidenceScore,
                ExistingMemoryItemId = x.ExistingMemoryItemId,
                ExistingMemoryContent = x.ExistingMemoryContent,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            }).ToList()
        };
    }
}
