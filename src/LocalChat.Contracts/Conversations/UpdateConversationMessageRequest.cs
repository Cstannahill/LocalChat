namespace LocalChat.Contracts.Conversations;

public sealed class UpdateConversationMessageRequest
{
    public required string Content { get; init; }

    public bool RegenerateAssistant { get; init; }
}
