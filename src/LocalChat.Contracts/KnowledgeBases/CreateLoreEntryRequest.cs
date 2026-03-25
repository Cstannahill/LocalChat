namespace LocalChat.Contracts.KnowledgeBases;

public sealed class CreateLoreEntryRequest
{
    public required Guid KnowledgeBaseId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public bool IsEnabled { get; init; } = true;
}
