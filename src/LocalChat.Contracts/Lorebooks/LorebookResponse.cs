namespace LocalChat.Contracts.Lorebooks;

public sealed class LorebookResponse
{
    public required Guid Id { get; init; }

    public Guid? CharacterId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public IReadOnlyList<LoreEntryResponse> Entries { get; init; } =
        Array.Empty<LoreEntryResponse>();
}
