namespace LocalChat.Application.Chat;

public sealed class SuggestedUserMessageResult
{
    public required Guid ConversationId { get; init; }

    public required string SuggestedMessage { get; init; }

    public string? Tone { get; init; }

    public string? ReasoningSummary { get; init; }
}
