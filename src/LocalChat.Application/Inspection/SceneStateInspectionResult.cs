namespace LocalChat.Application.Inspection;

public sealed class SceneStateInspectionResult
{
    public required Guid ConversationId { get; init; }

    public required IReadOnlyList<SceneStateDebugItem> ActiveSceneState { get; init; }

    public required IReadOnlyList<SceneStateReplacementHistoryItem> ReplacementHistory { get; init; }

    public required IReadOnlyList<SceneStateReplacementHistoryItem> FamilyCollisions { get; init; }
}
