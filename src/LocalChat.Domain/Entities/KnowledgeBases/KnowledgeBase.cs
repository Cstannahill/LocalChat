namespace LocalChat.Domain.Entities.KnowledgeBases;

public sealed class KnowledgeBase
{
    public Guid Id { get; set; }

    public Guid? AgentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<LoreEntry> Entries { get; set; } = new List<LoreEntry>();
}
