namespace LocalChat.Contracts.Settings;

public sealed class AppRuntimeDefaultsResponse
{
    public Guid? DefaultUserProfileId { get; init; }

    public Guid? DefaultModelProfileId { get; init; }

    public Guid? DefaultGenerationPresetId { get; init; }
}
