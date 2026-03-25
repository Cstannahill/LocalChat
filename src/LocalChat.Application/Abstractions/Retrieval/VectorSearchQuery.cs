namespace LocalChat.Application.Abstractions.Retrieval;

public sealed class VectorSearchQuery
{
    public required float[] QueryEmbedding { get; init; }

    public IReadOnlyList<string> SourceTypes { get; init; } = Array.Empty<string>();

    public Guid? AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public bool IncludeGlobalAgentItems { get; init; } = true;

    public bool IncludeGlobalConversationItems { get; init; } = true;

    public int TopK { get; init; } = 20;
}
