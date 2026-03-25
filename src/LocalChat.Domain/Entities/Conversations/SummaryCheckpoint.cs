namespace LocalChat.Domain.Entities.Conversations;

public sealed class SummaryCheckpoint
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public int StartSequenceNumber { get; set; }

    public int EndSequenceNumber { get; set; }

    public string SummaryText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid? ReplacedByCheckpointId { get; set; }

    public Conversation? Conversation { get; set; }
}