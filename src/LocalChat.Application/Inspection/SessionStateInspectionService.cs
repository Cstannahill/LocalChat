using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Inspection;

public sealed class SessionStateInspectionService : ISessionStateInspectionService
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly ISessionStateExtractionEventRepository _eventRepository;

    public SessionStateInspectionService(
        IMemoryRepository memoryRepository,
        ISessionStateExtractionEventRepository eventRepository)
    {
        _memoryRepository = memoryRepository;
        _eventRepository = eventRepository;
    }

    public async Task<SessionStateInspectionResult> InspectConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var memories = await _memoryRepository.ListByConversationAsync(conversationId, cancellationToken);
        var events = await _eventRepository.ListByConversationAsync(conversationId, 200, cancellationToken);

        var activeSessionState = memories
            .Where(x =>
                x.Kind == MemoryKind.SessionState &&
                x.ReviewStatus == MemoryReviewStatus.Accepted &&
                !x.SupersededAtSequenceNumber.HasValue)
            .OrderBy(x => x.SlotFamily.ToString())
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new SessionStateDebugItem
            {
                MemoryId = x.Id,
                Content = x.Content,
                SlotFamily = x.SlotFamily.ToString(),
                SlotKey = x.SlotKey,
                ReviewStatus = x.ReviewStatus.ToString(),
                ExpiresAt = x.ExpiresAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToList();

        var replacementHistory = events
            .Where(x => x.Action == "ReplacedBySlot" || x.Action == "ReplacedByFamily")
            .Select(x => new SessionStateReplacementHistoryItem
            {
                EventId = x.Id,
                SlotFamily = x.SlotFamily.ToString(),
                SlotKey = x.SlotKey,
                CandidateContent = x.CandidateContent,
                Action = x.Action,
                ReplacedMemoryItemId = x.ReplacedMemoryItemId,
                ReplacedMemoryContent = x.ReplacedMemoryContent,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        var familyCollisions = events
            .Where(x => x.Action == "ReplacedByFamily")
            .Select(x => new SessionStateReplacementHistoryItem
            {
                EventId = x.Id,
                SlotFamily = x.SlotFamily.ToString(),
                SlotKey = x.SlotKey,
                CandidateContent = x.CandidateContent,
                Action = x.Action,
                ReplacedMemoryItemId = x.ReplacedMemoryItemId,
                ReplacedMemoryContent = x.ReplacedMemoryContent,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return new SessionStateInspectionResult
        {
            ConversationId = conversationId,
            ActiveSessionState = activeSessionState,
            ReplacementHistory = replacementHistory,
            FamilyCollisions = familyCollisions
        };
    }
}
