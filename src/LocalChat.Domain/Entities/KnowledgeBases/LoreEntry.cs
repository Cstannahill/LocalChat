namespace LocalChat.Domain.Entities.KnowledgeBases;

public sealed class LoreEntry
{
    public Guid Id { get; set; }

    public Guid KnowledgeBaseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public KnowledgeBase? KnowledgeBase { get; set; }
}
