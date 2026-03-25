namespace LocalChat.Contracts.Lorebooks;

public sealed class LoreEntryResponse
{
    public required Guid Id { get; init; }

    public required Guid LorebookId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public required bool IsEnabled { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }
}
