namespace LocalChat.Contracts.Memory;

public sealed class MemoryConflictSuggestionResponse
{
    public required Guid SourceMemoryId { get; init; }

    public required Guid TargetMemoryId { get; init; }

    public required string SuggestedStrategy { get; init; }

    public required string Reason { get; init; }

    public required double TargetScore { get; init; }

    public required IReadOnlyList<string> RankingExplanation { get; init; }

    public required MemoryConflictMemorySummaryResponse Source { get; init; }

    public required MemoryConflictMemorySummaryResponse Target { get; init; }
}

public sealed class MemoryConflictMemorySummaryResponse
{
    public required Guid Id { get; init; }

    public required string ScopeType { get; init; }

    public required string Kind { get; init; }

    public string? Category { get; init; }

    public string? NormalizedKey { get; init; }

    public string Content { get; init; } = string.Empty;

    public Guid? ConversationId { get; init; }

    public Guid? AgentId { get; init; }

    public int? SourceMessageSequenceNumber { get; init; }

    public int? LastObservedSequenceNumber { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
