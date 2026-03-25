namespace LocalChat.Contracts.Personas;

public sealed class UserPersonaResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string Description { get; init; }

    public required string Traits { get; init; }

    public required string Preferences { get; init; }

    public required string AdditionalInstructions { get; init; }

    public required bool IsDefault { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }
}
