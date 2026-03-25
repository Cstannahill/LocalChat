namespace LocalChat.Application.Inspection;

public interface ISceneStateInspectionService
{
    Task<SceneStateInspectionResult> InspectConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
