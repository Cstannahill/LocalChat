namespace LocalChat.Contracts.Admin;

public sealed class RetrievalReindexResponse
{
    public required string Scope { get; init; }

    public Guid? ConversationId { get; init; }

    public required int UpdatedChunkCount { get; init; }

    public required int SkippedChunkCount { get; init; }

    public required long DurationMs { get; init; }
}
