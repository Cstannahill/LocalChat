namespace LocalChat.Contracts.Personas;

public sealed class PersonaDeletePreviewResponse
{
    public required Guid PersonaId { get; init; }

    public required string DisplayName { get; init; }

    public required bool IsDefault { get; init; }

    public required bool WillPromoteReplacement { get; init; }

    public Guid? ReplacementPersonaId { get; init; }

    public string? ReplacementDisplayName { get; init; }
}
