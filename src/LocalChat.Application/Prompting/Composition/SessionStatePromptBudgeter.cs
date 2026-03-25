using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Prompting.Composition;

public static class SessionStatePromptBudgeter
{
    public static SessionStatePromptSelectionResult Select(
        IReadOnlyList<MemoryItem> sessionStateItems,
        ITokenEstimator tokenEstimator,
        SessionStatePromptBudgetOptions? options = null)
    {
        options ??= new SessionStatePromptBudgetOptions();

        var selected = new List<SessionStatePromptSelectedItem>();
        var suppressed = new List<SessionStatePromptSuppressedItem>();

        var grouped = sessionStateItems
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
                suppressed.Add(new SessionStatePromptSuppressedItem
                {
                    MemoryId = older.Id,
                    SlotFamily = family,
                    Content = older.Content,
                    Reason = "Suppressed because a newer session-state item in the same family was selected first."
                });
            }

            var familyBudget = options.FamilyBudgets.TryGetValue(family, out var value)
                ? value
                : 22;

            var line = FormatLine(best, family);
            var trimmedLine = TrimToBudget(line, familyBudget, tokenEstimator);

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                suppressed.Add(new SessionStatePromptSuppressedItem
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
                suppressed.Add(new SessionStatePromptSuppressedItem
                {
                    MemoryId = best.Id,
                    SlotFamily = family,
                    Content = best.Content,
                    Reason = $"Suppressed because overall active session-state budget was already consumed by higher-priority families."
                });

                continue;
            }

            selected.Add(new SessionStatePromptSelectedItem
            {
                Memory = best,
                SlotFamily = family,
                PromptContent = trimmedLine
            });

            usedOverallTokens += tokens;
        }

        return new SessionStatePromptSelectionResult
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

public sealed class SessionStatePromptSelectionResult
{
    public required IReadOnlyList<SessionStatePromptSelectedItem> Selected { get; init; }

    public required IReadOnlyList<SessionStatePromptSuppressedItem> Suppressed { get; init; }
}

public sealed class SessionStatePromptSelectedItem
{
    public required MemoryItem Memory { get; init; }

    public required MemorySlotFamily SlotFamily { get; init; }

    public required string PromptContent { get; init; }
}

public sealed class SessionStatePromptSuppressedItem
{
    public required Guid MemoryId { get; init; }

    public required MemorySlotFamily SlotFamily { get; init; }

    public required string Content { get; init; }

    public required string Reason { get; init; }
}
