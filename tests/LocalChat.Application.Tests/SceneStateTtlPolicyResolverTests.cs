using LocalChat.Application.Features.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class SceneStateTtlPolicyResolverTests
{
    [Fact]
    public void ResolveExpiration_UsesFamilySpecificPolicy()
    {
        var options = new MemoryProposalOptions
        {
            SceneStateTtlHoursDefault = 8,
            SceneStateTtlHoursOutfit = 6,
            SceneStateTtlHoursLocation = 10,
            SceneStateTtlHoursEmotionalState = 2
        };

        var resolver = new SceneStateTtlPolicyResolver(options);
        var now = new DateTime(2026, 3, 17, 12, 0, 0, DateTimeKind.Utc);

        var outfit = resolver.ResolveExpiration(MemorySlotFamily.Outfit, now);
        var location = resolver.ResolveExpiration(MemorySlotFamily.Location, now);
        var emotion = resolver.ResolveExpiration(MemorySlotFamily.EmotionalState, now);

        Assert.Equal(now.AddHours(6), outfit);
        Assert.Equal(now.AddHours(10), location);
        Assert.Equal(now.AddHours(2), emotion);
    }
}
