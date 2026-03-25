using LocalChat.Application.Inspection;

namespace LocalChat.Application.Abstractions.Prompting;

public interface IPromptInspectionService
{
    Task<ContextInspectionResult> InspectAsync(
        Guid agentId,
        Guid? conversationId,
        Guid? userProfileId,
        string currentUserMessage,
        CancellationToken cancellationToken = default
    );
}
