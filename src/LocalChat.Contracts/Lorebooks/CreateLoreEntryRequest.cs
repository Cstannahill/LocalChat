namespace LocalChat.Contracts.Lorebooks;

public sealed class CreateLoreEntryRequest
{
    public required Guid LorebookId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public bool IsEnabled { get; init; } = true;
}
