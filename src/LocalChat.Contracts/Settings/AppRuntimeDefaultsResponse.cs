namespace LocalChat.Contracts.Settings;

public sealed class AppRuntimeDefaultsResponse
{
    public Guid? DefaultPersonaId { get; init; }

    public Guid? DefaultModelProfileId { get; init; }

    public Guid? DefaultGenerationPresetId { get; init; }
}
