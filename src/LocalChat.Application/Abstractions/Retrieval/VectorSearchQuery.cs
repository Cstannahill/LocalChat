namespace LocalChat.Application.Abstractions.Retrieval;

public sealed class VectorSearchQuery
{
    public required float[] QueryEmbedding { get; init; }

    public IReadOnlyList<string> SourceTypes { get; init; } = Array.Empty<string>();

    public Guid? CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public bool IncludeGlobalCharacterItems { get; init; } = true;

    public bool IncludeGlobalConversationItems { get; init; } = true;

    public int TopK { get; init; } = 20;
}
