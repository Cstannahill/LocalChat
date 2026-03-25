namespace LocalChat.Contracts.PromptSnapshots;

public sealed class PromptSnapshotSectionResponse
{
    public required string Name { get; init; }

    public required string Content { get; init; }

    public required int EstimatedTokens { get; init; }
}
