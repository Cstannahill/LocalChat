using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Prompting.Composition;

public static class SceneStatePromptBudgeter
{
    public static SceneStatePromptSelectionResult Select(
        IReadOnlyList<MemoryItem> sceneStateItems,
        ITokenEstimator tokenEstimator,
        SceneStatePromptBudgetOptions? options = null)
    {
        options ??= new SceneStatePromptBudgetOptions();

        var selected = new List<SceneStatePromptSelectedItem>();
        var suppressed = new List<SceneStatePromptSuppressedItem>();

        var grouped = sceneStateItems
            .GroupBy(x => x.SlotFamily == MemorySlotFamily.None ? MemorySlotFamily.Misc : x.SlotFamily)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.UpdatedAt).ToList());

        var usedOverallTokens = 0;

        foreach (var family in options.FamilyPriorityOrder)
        {
            if (!grouped.TryGetValue(family, out var items) || items.Count == 0)
            {
                continue;
            }

            var best = items[0];

            foreach (var older in items.Skip(1))
            {
                suppressed.Add(new SceneStatePromptSuppressedItem
                {
                    MemoryId = older.Id,
                    SlotFamily = family,
                    Content = older.Content,
                    Reason = "Suppressed because a newer scene-state item in the same family was selected first."
                });
            }

            var familyBudget = options.FamilyBudgets.TryGetValue(family, out var value)
                ? value
                : 22;

            var line = FormatLine(best, family);
            var trimmedLine = TrimToBudget(line, familyBudget, tokenEstimator);

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                suppressed.Add(new SceneStatePromptSuppressedItem
                {
                    MemoryId = best.Id,
                    SlotFamily = family,
                    Content = best.Content,
                    Reason = $"Suppressed because family budget for '{family}' could not fit the item."
                });

                continue;
            }

            var tokens = tokenEstimator.EstimateTokens(trimmedLine);

            if (selected.Count > 0 && usedOverallTokens + tokens > options.OverallBudgetTokens)
            {
                suppressed.Add(new SceneStatePromptSuppressedItem
                {
                    MemoryId = best.Id,
                    SlotFamily = family,
                    Content = best.Content,
                    Reason = $"Suppressed because overall active scene-state budget was already consumed by higher-priority families."
                });

                continue;
            }

            selected.Add(new SceneStatePromptSelectedItem
            {
                Memory = best,
                SlotFamily = family,
                PromptContent = trimmedLine
            });

            usedOverallTokens += tokens;
        }

        return new SceneStatePromptSelectionResult
        {
            Selected = selected,
            Suppressed = suppressed
        };
    }

    private static string FormatLine(MemoryItem item, MemorySlotFamily family)
    {
        return $"- [{family}] {item.Content}";
    }

    private static string TrimToBudget(
        string text,
        int tokenBudget,
        ITokenEstimator tokenEstimator)
    {
        if (tokenEstimator.EstimateTokens(text) <= tokenBudget)
        {
            return text;
        }

        var words = text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        while (words.Count > 0)
        {
            var candidate = string.Join(' ', words) + " ...";
            if (tokenEstimator.EstimateTokens(candidate) <= tokenBudget)
            {
                return candidate;
            }

            words.RemoveAt(words.Count - 1);
        }

        return string.Empty;
    }
}

public sealed class SceneStatePromptSelectionResult
{
    public required IReadOnlyList<SceneStatePromptSelectedItem> Selected { get; init; }

    public required IReadOnlyList<SceneStatePromptSuppressedItem> Suppressed { get; init; }
}

public sealed class SceneStatePromptSelectedItem
{
    public required MemoryItem Memory { get; init; }

    public required MemorySlotFamily SlotFamily { get; init; }

    public required string PromptContent { get; init; }
}

public sealed class SceneStatePromptSuppressedItem
{
    public required Guid MemoryId { get; init; }

    public required MemorySlotFamily SlotFamily { get; init; }

    public required string Content { get; init; }

    public required string Reason { get; init; }
}
