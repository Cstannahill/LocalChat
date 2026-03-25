using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class SceneStatePromptBudgeterTests
{
    [Fact]
    public void Select_PrefersHigherPriorityFamilies_WhenBudgetIsTight()
    {
        var memories = new List<MemoryItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Kind = MemoryKind.SceneState,
                Category = MemoryCategory.SceneState,
                Content = "They are on a balcony.",
                SlotFamily = MemorySlotFamily.Location,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Kind = MemoryKind.SceneState,
                Category = MemoryCategory.SceneState,
                Content = "They are standing close together.",
                SlotFamily = MemorySlotFamily.PoseAction,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-1),
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Kind = MemoryKind.SceneState,
                Category = MemoryCategory.SceneState,
                Content = "She is wearing a yellow sundress.",
                SlotFamily = MemorySlotFamily.Outfit,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-2),
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            }
        };

        var result = SceneStatePromptBudgeter.Select(
            memories,
            new FakeTokenEstimator(),
            new SceneStatePromptBudgetOptions
            {
                OverallBudgetTokens = 12,
                FamilyBudgets = new Dictionary<MemorySlotFamily, int>
                {
                    [MemorySlotFamily.Location] = 6,
                    [MemorySlotFamily.PoseAction] = 6,
                    [MemorySlotFamily.Outfit] = 6,
                    [MemorySlotFamily.EmotionalState] = 6,
                    [MemorySlotFamily.Possession] = 6,
                    [MemorySlotFamily.RelationshipState] = 6,
                    [MemorySlotFamily.Misc] = 6,
                    [MemorySlotFamily.None] = 6
                }
            });

        Assert.Equal(2, result.Selected.Count);
        Assert.Contains(result.Selected, x => x.SlotFamily == MemorySlotFamily.Location);
        Assert.Contains(result.Selected, x => x.SlotFamily == MemorySlotFamily.PoseAction);
        Assert.DoesNotContain(result.Selected, x => x.SlotFamily == MemorySlotFamily.Outfit);
        Assert.Contains(result.Suppressed, x => x.SlotFamily == MemorySlotFamily.Outfit);
    }

    [Fact]
    public void Select_KeepsNewestItemPerFamily()
    {
        var older = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Kind = MemoryKind.SceneState,
            Category = MemoryCategory.SceneState,
            Content = "Yellow sundress.",
            SlotFamily = MemorySlotFamily.Outfit,
            UpdatedAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var newer = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Kind = MemoryKind.SceneState,
            Category = MemoryCategory.SceneState,
            Content = "Black evening gown.",
            SlotFamily = MemorySlotFamily.Outfit,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        };

        var result = SceneStatePromptBudgeter.Select(
            new[] { older, newer },
            new FakeTokenEstimator());

        Assert.Single(result.Selected);
        Assert.Equal(newer.Id, result.Selected[0].Memory.Id);
        Assert.Single(result.Suppressed);
        Assert.Equal(older.Id, result.Suppressed[0].MemoryId);
    }

    private sealed class FakeTokenEstimator : ITokenEstimator
    {
        public int EstimateTokens(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
