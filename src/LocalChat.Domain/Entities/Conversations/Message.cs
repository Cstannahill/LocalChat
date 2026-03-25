using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Conversations;

public sealed class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public MessageRole Role { get; set; }

    public MessageOriginType OriginType { get; set; } = MessageOriginType.User;

    public string Content { get; set; } = string.Empty;

    public int SequenceNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? SelectedVariantIndex { get; set; }

    public Conversation? Conversation { get; set; }

    public ICollection<MessageVariant> Variants { get; set; } = new List<MessageVariant>();
}
