namespace LocalChat.Contracts.KnowledgeBases;

public sealed class KnowledgeBaseResponse
{
    public required Guid Id { get; init; }

    public Guid? AgentId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public IReadOnlyList<LoreEntryResponse> Entries { get; init; } =
        Array.Empty<LoreEntryResponse>();
}
