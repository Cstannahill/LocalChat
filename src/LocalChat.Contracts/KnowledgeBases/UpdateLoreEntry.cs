namespace LocalChat.Contracts.KnowledgeBases;

public sealed class UpdateLoreEntryRequest
{
    public required string Title { get; init; }

    public required string Content { get; init; }

    public required bool IsEnabled { get; init; }
}
