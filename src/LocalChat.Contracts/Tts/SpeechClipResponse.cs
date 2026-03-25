namespace LocalChat.Contracts.Tts;

public sealed class SpeechClipResponse
{
    public required Guid Id { get; init; }

    public required Guid CharacterId { get; init; }

    public required Guid ConversationId { get; init; }

    public required Guid MessageId { get; init; }

    public required string Provider { get; init; }

    public required string Voice { get; init; }

    public required string ModelIdentifier { get; init; }

    public required string ResponseFormat { get; init; }

    public required string ContentType { get; init; }

    public required string Url { get; init; }

    public required string SourceText { get; init; }

    public required DateTime CreatedAt { get; init; }
}
