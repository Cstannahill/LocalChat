namespace LocalChat.Application.Chat;

public sealed class RegenerateAssistantMessageResult
{
    public required Guid ConversationId { get; init; }

    public required Guid MessageId { get; init; }

    public required string AssistantMessage { get; init; }

    public required int SelectedVariantIndex { get; init; }

    public required int VariantCount { get; init; }
}
