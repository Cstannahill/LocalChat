namespace LocalChat.Application.Inspection;

public sealed class RetrievalCandidateDebug
{
    public required Guid SourceId { get; init; }

    public required string SourceType { get; init; }

    public required string PreviewText { get; init; }

    public required double SemanticScore { get; init; }

    public required double LexicalScore { get; init; }

    public required double RecencyScore { get; init; }

    public required double SourceBoost { get; init; }

    public required double FinalScore { get; init; }

    public required bool Selected { get; init; }

    public required string Reason { get; init; }
}
