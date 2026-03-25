namespace LocalChat.Contracts.Chat;

public sealed class RegenerateAssistantMessageResponse
{
    public required Guid ConversationId { get; init; }

    public required Guid MessageId { get; init; }

    public required string AssistantMessage { get; init; }

    public required int SelectedVariantIndex { get; init; }

    public required int VariantCount { get; init; }
}
