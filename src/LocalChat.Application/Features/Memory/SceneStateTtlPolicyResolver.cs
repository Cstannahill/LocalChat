using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class SceneStateTtlPolicyResolver
{
    private readonly MemoryProposalOptions _options;

    public SceneStateTtlPolicyResolver(MemoryProposalOptions options)
    {
        _options = options;
    }

    public DateTime ResolveExpiration(MemorySlotFamily slotFamily, DateTime utcNow)
    {
        var hours = slotFamily switch
        {
            MemorySlotFamily.Outfit => _options.SceneStateTtlHoursOutfit,
            MemorySlotFamily.Location => _options.SceneStateTtlHoursLocation,
            MemorySlotFamily.PoseAction => _options.SceneStateTtlHoursPoseAction,
            MemorySlotFamily.Possession => _options.SceneStateTtlHoursPossession,
            MemorySlotFamily.EmotionalState => _options.SceneStateTtlHoursEmotionalState,
            MemorySlotFamily.RelationshipState => _options.SceneStateTtlHoursRelationshipState,
            MemorySlotFamily.Misc => _options.SceneStateTtlHoursMisc,
            MemorySlotFamily.None => _options.SceneStateTtlHoursDefault,
            _ => _options.SceneStateTtlHoursDefault
        };

        return utcNow.AddHours(hours);
    }
}
