using LocalChat.Application.Features.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class MemoryProposalQualityEvaluatorTests
{
    [Fact]
    public void BuildSlotFamily_DetectsSceneOutfit()
    {
        var evaluator = new MemoryProposalQualityEvaluator();

        var family = evaluator.BuildSlotFamily(
            MemoryCategory.SessionState,
            "The agent is currently wearing a yellow sundress.",
            null,
            null);

        Assert.Equal(MemorySlotFamily.Outfit, family);
    }

    [Fact]
    public void BuildSlotFamily_DetectsSceneLocation()
    {
        var evaluator = new MemoryProposalQualityEvaluator();

        var family = evaluator.BuildSlotFamily(
            MemoryCategory.SessionState,
            "They are currently standing on a balcony.",
            null,
            null);

        Assert.Equal(MemorySlotFamily.Location, family);
    }

    [Fact]
    public void BuildSlotFamily_DetectsSceneEmotion()
    {
        var evaluator = new MemoryProposalQualityEvaluator();

        var family = evaluator.BuildSlotFamily(
            MemoryCategory.SessionState,
            "The agent is visibly nervous right now.",
            null,
            null);

        Assert.Equal(MemorySlotFamily.EmotionalState, family);
    }
}
