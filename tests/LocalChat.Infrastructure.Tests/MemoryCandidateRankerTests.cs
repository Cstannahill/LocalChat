using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using LocalChat.Application.Memory;
using LocalChat.Infrastructure.Retrieval;

namespace LocalChat.Infrastructure.Tests;

public sealed class MemoryCandidateRankerTests
{
    [Fact]
    public void Rank_PrefersActiveSceneState_OverDurableFact_InSameSlot_AndExplainsSuppression()
    {
        var conversationId = Guid.NewGuid();

        var durable = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Category = MemoryCategory.CharacterFact,
            Kind = MemoryKind.DurableFact,
            Content = "The character usually dresses elegantly.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            SlotKey = "scene.character.outfit",
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var scene = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Category = MemoryCategory.SceneState,
            Kind = MemoryKind.SceneState,
            Content = "The character is currently wearing a yellow sundress.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            ConversationId = conversationId,
            SlotKey = "scene.character.outfit",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var ranking = MemoryCandidateRanker.Rank(
            new[] { durable, scene },
            new Dictionary<Guid, double>
            {
                [durable.Id] = 0.81,
                [scene.Id] = 0.76
            },
            conversationId,
            currentSequenceNumber: 20,
            memoryPolicyService: new MemoryPolicyService(),
            maxCount: 8);

        Assert.Single(ranking.Selected);

        var selected = ranking.Selected[0];
        Assert.Equal(scene.Id, selected.Memory.Id);
        Assert.Equal("scene.character.outfit", selected.Memory.SlotKey);
        Assert.Contains("Suppressed 1 lower-ranked memory item", selected.WhySelected);
        Assert.Single(selected.SuppressedMemories);
        Assert.Equal(durable.Id, selected.SuppressedMemories[0].Memory.Id);
    }

    [Fact]
    public void Rank_ChoosesNewestSceneState_ForSameSlot()
    {
        var conversationId = Guid.NewGuid();

        var olderScene = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Category = MemoryCategory.SceneState,
            Kind = MemoryKind.SceneState,
            Content = "The character is currently wearing a yellow sundress.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            ConversationId = conversationId,
            SlotKey = "scene.character.outfit",
            SupersededAtSequenceNumber = 18,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            UpdatedAt = DateTime.UtcNow.AddHours(-2),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var newerScene = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Category = MemoryCategory.SceneState,
            Kind = MemoryKind.SceneState,
            Content = "The character is currently wearing a black evening gown.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            ConversationId = conversationId,
            SlotKey = "scene.character.outfit",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-2),
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
        };

        var ranking = MemoryCandidateRanker.Rank(
            new[] { olderScene, newerScene },
            new Dictionary<Guid, double>
            {
                [olderScene.Id] = 0.79,
                [newerScene.Id] = 0.77
            },
            conversationId,
            currentSequenceNumber: 20,
            memoryPolicyService: new MemoryPolicyService(),
            maxCount: 8);

        Assert.Single(ranking.Selected);
        Assert.Equal(newerScene.Id, ranking.Selected[0].Memory.Id);
    }

    [Fact]
    public void Rank_KeepsPinnedDurableFact_WhenNoSceneStateClaimsItsSlot()
    {
        var conversationId = Guid.NewGuid();

        var durable = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Category = MemoryCategory.UserFact,
            Kind = MemoryKind.DurableFact,
            Content = "The user prefers TypeScript over JavaScript.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            SlotKey = "user.preference",
            IsPinned = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var scene = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Category = MemoryCategory.SceneState,
            Kind = MemoryKind.SceneState,
            Content = "They are currently standing on a balcony.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            ConversationId = conversationId,
            SlotKey = "scene.location",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-3),
            CreatedAt = DateTime.UtcNow.AddMinutes(-3)
        };

        var ranking = MemoryCandidateRanker.Rank(
            new[] { durable, scene },
            new Dictionary<Guid, double>
            {
                [durable.Id] = 0.70,
                [scene.Id] = 0.74
            },
            conversationId,
            currentSequenceNumber: 20,
            memoryPolicyService: new MemoryPolicyService(),
            maxCount: 8);

        Assert.Equal(2, ranking.Selected.Count);
        Assert.Contains(ranking.Selected, x => x.Memory.Id == durable.Id);
        Assert.Contains(ranking.Selected, x => x.Memory.Id == scene.Id);
    }
}
