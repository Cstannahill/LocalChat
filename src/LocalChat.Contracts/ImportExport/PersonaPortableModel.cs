namespace LocalChat.Contracts.ImportExport;

public sealed class PersonaPortableModel
{
    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string Description { get; init; }

    public required string Traits { get; init; }

    public required string Preferences { get; init; }

    public required string AdditionalInstructions { get; init; }
}
