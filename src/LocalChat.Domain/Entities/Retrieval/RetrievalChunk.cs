namespace LocalChat.Domain.Entities.Retrieval;

public sealed class RetrievalChunk
{
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Guid? ConversationId { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public Guid SourceEntityId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string EmbeddingJson { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
