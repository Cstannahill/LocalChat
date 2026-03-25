namespace LocalChat.Application.Inspection;

public interface ISessionStateInspectionService
{
    Task<SessionStateInspectionResult> InspectConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
