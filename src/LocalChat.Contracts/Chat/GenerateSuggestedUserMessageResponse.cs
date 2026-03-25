namespace LocalChat.Contracts.Chat;

public sealed class GenerateSuggestedUserMessageResponse
{
    public required Guid ConversationId { get; init; }

    public required string SuggestedMessage { get; init; }

    public string? Tone { get; init; }

    public string? ReasoningSummary { get; init; }
}
