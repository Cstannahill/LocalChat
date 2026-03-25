namespace LocalChat.Contracts.Inspection;

public sealed class SessionStateInspectionResponse
{
    public required Guid ConversationId { get; init; }

    public required IReadOnlyList<SessionStateDebugItemResponse> ActiveSessionState { get; init; }

    public required IReadOnlyList<SessionStateReplacementHistoryItemResponse> ReplacementHistory { get; init; }

    public required IReadOnlyList<SessionStateReplacementHistoryItemResponse> FamilyCollisions { get; init; }
}
