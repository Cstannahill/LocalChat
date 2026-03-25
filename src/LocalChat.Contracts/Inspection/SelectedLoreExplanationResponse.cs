namespace LocalChat.Contracts.Inspection;

public sealed class SelectedLoreExplanationResponse
{
    public required Guid LoreEntryId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public required double SemanticScore { get; init; }

    public required double FinalScore { get; init; }

    public required string WhySelected { get; init; }
}
