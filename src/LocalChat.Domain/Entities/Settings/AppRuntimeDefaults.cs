namespace LocalChat.Domain.Entities.Settings;

public sealed class AppRuntimeDefaults
{
    public Guid Id { get; set; }

    public Guid? DefaultUserProfileId { get; set; }

    public Guid? DefaultModelProfileId { get; set; }

    public Guid? DefaultGenerationPresetId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
