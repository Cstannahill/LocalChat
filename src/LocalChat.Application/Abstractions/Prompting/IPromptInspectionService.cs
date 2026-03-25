using LocalChat.Application.Inspection;

namespace LocalChat.Application.Abstractions.Prompting;

public interface IPromptInspectionService
{
    Task<ContextInspectionResult> InspectAsync(
        Guid characterId,
        Guid? conversationId,
        Guid? userPersonaId,
        string currentUserMessage,
        CancellationToken cancellationToken = default
    );
}
