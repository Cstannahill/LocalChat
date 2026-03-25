namespace LocalChat.Application.Abstractions.Retrieval;

public sealed class RetrievalReindexResult
{
    public required string Scope { get; init; }

    public Guid? ConversationId { get; init; }

    public required int UpdatedChunkCount { get; init; }

    public required int SkippedChunkCount { get; init; }

    public required long DurationMs { get; init; }
}
