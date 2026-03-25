namespace LocalChat.Contracts.Lorebooks;

public sealed class CreateLorebookRequest
{
    public Guid? CharacterId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }
}
