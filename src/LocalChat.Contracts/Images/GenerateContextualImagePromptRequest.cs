namespace LocalChat.Contracts.Images;

public sealed class GenerateContextualImagePromptRequest
{
    public required Guid ConversationId { get; init; }
}
