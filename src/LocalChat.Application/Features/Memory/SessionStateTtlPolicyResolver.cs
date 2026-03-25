using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class SessionStateTtlPolicyResolver
{
    private readonly MemoryProposalOptions _options;

    public SessionStateTtlPolicyResolver(MemoryProposalOptions options)
    {
        _options = options;
    }

    public DateTime ResolveExpiration(MemorySlotFamily slotFamily, DateTime utcNow)
    {
        var hours = slotFamily switch
        {
            MemorySlotFamily.Outfit => _options.SessionStateTtlHoursOutfit,
            MemorySlotFamily.Location => _options.SessionStateTtlHoursLocation,
            MemorySlotFamily.PoseAction => _options.SessionStateTtlHoursPoseAction,
            MemorySlotFamily.Possession => _options.SessionStateTtlHoursPossession,
            MemorySlotFamily.EmotionalState => _options.SessionStateTtlHoursEmotionalState,
            MemorySlotFamily.RelationshipState => _options.SessionStateTtlHoursRelationshipState,
            MemorySlotFamily.Misc => _options.SessionStateTtlHoursMisc,
            MemorySlotFamily.None => _options.SessionStateTtlHoursDefault,
            _ => _options.SessionStateTtlHoursDefault
        };

        return utcNow.AddHours(hours);
    }
}
