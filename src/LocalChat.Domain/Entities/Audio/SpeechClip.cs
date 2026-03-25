using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Domain.Entities.Audio;

public sealed class SpeechClip
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

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

    public Agent? Agent { get; set; }

    public Conversation? Conversation { get; set; }

    public Message? Message { get; set; }
}
