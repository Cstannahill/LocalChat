namespace LocalChat.Application.Abstractions.Retrieval;

public sealed class VectorSearchResult
{
    public required Guid SourceId { get; init; }

    public required string SourceType { get; init; }

    public Guid? CharacterId { get; init; }

    public Guid? ConversationId { get; init; }

    public required string Content { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required double SemanticScore { get; init; }
}
