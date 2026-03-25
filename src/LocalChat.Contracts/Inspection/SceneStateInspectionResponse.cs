namespace LocalChat.Contracts.Inspection;

public sealed class SceneStateInspectionResponse
{
    public required Guid ConversationId { get; init; }

    public required IReadOnlyList<SceneStateDebugItemResponse> ActiveSceneState { get; init; }

    public required IReadOnlyList<SceneStateReplacementHistoryItemResponse> ReplacementHistory { get; init; }

    public required IReadOnlyList<SceneStateReplacementHistoryItemResponse> FamilyCollisions { get; init; }
}
