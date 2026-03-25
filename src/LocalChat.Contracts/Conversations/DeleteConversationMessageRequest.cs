namespace LocalChat.Contracts.Conversations;

public sealed class DeleteConversationMessageRequest
{
    public bool RegenerateAssistant { get; init; }
}
