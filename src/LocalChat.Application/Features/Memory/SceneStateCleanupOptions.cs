using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class SceneStateCleanupOptions
{
    public bool Enabled { get; set; } = true;

    public bool PreservePinned { get; set; } = true;

    public int DefaultMaxAgeHours { get; set; } = 24;

    public int OutfitMaxAgeHours { get; set; } = 18;

    public int LocationMaxAgeHours { get; set; } = 36;

    public int PoseActionMaxAgeHours { get; set; } = 6;

    public int PossessionMaxAgeHours { get; set; } = 12;

    public int EmotionalStateMaxAgeHours { get; set; } = 3;

    public int RelationshipStateMaxAgeHours { get; set; } = 48;

    public int MiscMaxAgeHours { get; set; } = 12;

    public TimeSpan ResolveMaxAge(MemorySlotFamily family) =>
        family switch
        {
            MemorySlotFamily.Outfit => TimeSpan.FromHours(OutfitMaxAgeHours),
            MemorySlotFamily.Location => TimeSpan.FromHours(LocationMaxAgeHours),
            MemorySlotFamily.PoseAction => TimeSpan.FromHours(PoseActionMaxAgeHours),
            MemorySlotFamily.Possession => TimeSpan.FromHours(PossessionMaxAgeHours),
            MemorySlotFamily.EmotionalState => TimeSpan.FromHours(EmotionalStateMaxAgeHours),
            MemorySlotFamily.RelationshipState => TimeSpan.FromHours(RelationshipStateMaxAgeHours),
            MemorySlotFamily.Misc => TimeSpan.FromHours(MiscMaxAgeHours),
            MemorySlotFamily.None => TimeSpan.FromHours(DefaultMaxAgeHours),
            _ => TimeSpan.FromHours(DefaultMaxAgeHours)
        };
}
