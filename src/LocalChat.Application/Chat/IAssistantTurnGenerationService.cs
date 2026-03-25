namespace LocalChat.Application.Chat;

public interface IAssistantTurnGenerationService
{
    Task<GeneratedAssistantTurnResult> GenerateAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
