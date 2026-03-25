namespace LocalChat.Application.Inspection;

public sealed class SessionStateInspectionResult
{
    public required Guid ConversationId { get; init; }

    public required IReadOnlyList<SessionStateDebugItem> ActiveSessionState { get; init; }

    public required IReadOnlyList<SessionStateReplacementHistoryItem> ReplacementHistory { get; init; }

    public required IReadOnlyList<SessionStateReplacementHistoryItem> FamilyCollisions { get; init; }
}
