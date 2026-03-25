using LocalChat.Domain.Enums;

namespace LocalChat.Application.Prompting.Composition;

public sealed class SceneStatePromptBudgetOptions
{
    public int OverallBudgetTokens { get; init; } = 180;

    public IReadOnlyList<MemorySlotFamily> FamilyPriorityOrder { get; init; } =
    [
        MemorySlotFamily.Location,
        MemorySlotFamily.PoseAction,
        MemorySlotFamily.EmotionalState,
        MemorySlotFamily.Outfit,
        MemorySlotFamily.Possession,
        MemorySlotFamily.RelationshipState,
        MemorySlotFamily.Misc,
        MemorySlotFamily.None
    ];

    public IReadOnlyDictionary<MemorySlotFamily, int> FamilyBudgets { get; init; } =
        new Dictionary<MemorySlotFamily, int>
        {
            [MemorySlotFamily.Location] = 40,
            [MemorySlotFamily.PoseAction] = 44,
            [MemorySlotFamily.EmotionalState] = 32,
            [MemorySlotFamily.Outfit] = 34,
            [MemorySlotFamily.Possession] = 28,
            [MemorySlotFamily.RelationshipState] = 26,
            [MemorySlotFamily.Misc] = 22,
            [MemorySlotFamily.None] = 22
        };
}
