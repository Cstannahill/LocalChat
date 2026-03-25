using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Domain.Entities.Audio;

public sealed class SpeechClip
{
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid MessageId { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Voice { get; set; } = string.Empty;

    public string ModelIdentifier { get; set; } = string.Empty;

    public string ResponseFormat { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string RelativeUrl { get; set; } = string.Empty;

    public string SourceText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Character? Character { get; set; }

    public Conversation? Conversation { get; set; }

    public Message? Message { get; set; }
}
