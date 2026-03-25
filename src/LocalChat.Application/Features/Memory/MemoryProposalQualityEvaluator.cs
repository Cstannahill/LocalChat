using System.Text;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class MemoryProposalQualityEvaluator
{
    private static readonly string[] OutfitWords =
    [
        "wearing", "dress", "shirt", "jacket", "coat", "skirt", "sundress", "outfit", "gown", "uniform", "heels", "boots"
    ];

    private static readonly string[] LocationWords =
    [
        "at the", "in the", "on the", "inside", "outside", "balcony", "bar", "beach", "bedroom", "room", "forest", "street", "kitchen"
    ];

    private static readonly string[] HoldingWords =
    [
        "holding", "carrying", "gripping", "has a", "with a", "with an"
    ];

    private static readonly string[] PoseActionWords =
    [
        "sitting", "standing", "leaning", "kneeling", "lying", "hugging", "kissing", "embracing", "touching", "walking", "dancing"
    ];

    private static readonly string[] EmotionalWords =
    [
        "happy", "sad", "angry", "nervous", "ashamed", "embarrassed", "afraid", "anxious", "jealous", "calm", "excited", "tense", "frustrated"
    ];

    private static readonly string[] PreferenceWords =
    [
        "prefers", "likes", "favorite", "enjoys", "loves", "hates", "dislikes"
    ];

    public string NormalizeKey(MemoryCategory category, string content)
    {
        var normalized = NormalizeInline(content);
        return $"{category}:{normalized}";
    }

    public string BuildSlotKey(
        MemoryCategory category,
        string content,
        string? proposedSlotKey)
    {
        if (!string.IsNullOrWhiteSpace(proposedSlotKey))
        {
            return NormalizeSlotKey(proposedSlotKey!);
        }

        var family = BuildSlotFamily(category, content, proposedSlotKey, null);
        var actor = DetectActor(content, category);

        return family switch
        {
            MemorySlotFamily.Outfit => $"scene.{actor}.outfit",
            MemorySlotFamily.Location => "scene.location",
            MemorySlotFamily.PoseAction => $"scene.{actor}.pose-action",
            MemorySlotFamily.Possession => $"scene.{actor}.possession",
            MemorySlotFamily.EmotionalState => $"scene.{actor}.emotion",
            MemorySlotFamily.Preference when category == MemoryCategory.UserFact => "user.preference",
            MemorySlotFamily.Preference when category == MemoryCategory.CharacterFact => "character.preference",
            MemorySlotFamily.RelationshipState => "relationship.status",
            MemorySlotFamily.Identity when category == MemoryCategory.UserFact => "user.identity",
            MemorySlotFamily.Identity when category == MemoryCategory.CharacterFact => "character.identity",
            MemorySlotFamily.WorldState => "world.state",
            _ => category switch
            {
                MemoryCategory.UserFact => "user.fact",
                MemoryCategory.CharacterFact => "character.fact",
                MemoryCategory.RelationshipFact => "relationship.status",
                MemoryCategory.WorldFact => "world.fact",
                MemoryCategory.SceneState => "scene.misc",
                _ => "memory.misc"
            }
        };
    }

    public MemorySlotFamily BuildSlotFamily(
        MemoryCategory category,
        string content,
        string? proposedSlotKey,
        string? proposedSlotFamily)
    {
        if (!string.IsNullOrWhiteSpace(proposedSlotFamily) &&
            Enum.TryParse<MemorySlotFamily>(proposedSlotFamily, true, out var parsedFamily))
        {
            return parsedFamily;
        }

        var lower = content.ToLowerInvariant();

        if (category == MemoryCategory.SceneState)
        {
            if (ContainsAny(lower, OutfitWords)) return MemorySlotFamily.Outfit;
            if (ContainsAny(lower, LocationWords)) return MemorySlotFamily.Location;
            if (ContainsAny(lower, HoldingWords)) return MemorySlotFamily.Possession;
            if (ContainsAny(lower, PoseActionWords)) return MemorySlotFamily.PoseAction;
            if (ContainsAny(lower, EmotionalWords)) return MemorySlotFamily.EmotionalState;
            return MemorySlotFamily.Misc;
        }

        if (category == MemoryCategory.UserFact || category == MemoryCategory.CharacterFact)
        {
            if (ContainsAny(lower, PreferenceWords)) return MemorySlotFamily.Preference;
            return MemorySlotFamily.Identity;
        }

        if (category == MemoryCategory.RelationshipFact)
        {
            return MemorySlotFamily.RelationshipState;
        }

        if (category == MemoryCategory.WorldFact)
        {
            return MemorySlotFamily.WorldState;
        }

        return MemorySlotFamily.Misc;
    }

    public bool IsNearDuplicate(
        string normalizedKey,
        IReadOnlyList<MemoryItem> existing)
    {
        return existing.Any(x =>
            !string.IsNullOrWhiteSpace(x.NormalizedKey) &&
            string.Equals(x.NormalizedKey, normalizedKey, StringComparison.Ordinal));
    }

    public MemoryItem? FindLikelyConflict(
        MemoryCategory category,
        string normalizedKey,
        string slotKey,
        IReadOnlyList<MemoryItem> existing)
    {
        return existing.FirstOrDefault(x =>
            x.Category == category &&
            !string.IsNullOrWhiteSpace(x.SlotKey) &&
            string.Equals(x.SlotKey, slotKey, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(x.NormalizedKey) &&
            !string.Equals(x.NormalizedKey, normalizedKey, StringComparison.Ordinal));
    }

    public bool ShouldMergeIntoExistingProposal(
        MemoryItem existing,
        ExtractedMemoryCandidate candidate)
    {
        if (existing.ReviewStatus != MemoryReviewStatus.Proposed)
        {
            return false;
        }

        var existingConfidence = existing.ConfidenceScore ?? 0.0;
        return candidate.ConfidenceScore >= existingConfidence;
    }

    private static string DetectActor(string content, MemoryCategory category)
    {
        var lower = content.ToLowerInvariant();

        if (category == MemoryCategory.UserFact || lower.Contains("user"))
        {
            return "user";
        }

        if (lower.Contains("character") || lower.Contains("she") || lower.Contains("he"))
        {
            return "character";
        }

        return "shared";
    }

    private static bool ContainsAny(string lower, IEnumerable<string> patterns)
    {
        return patterns.Any(lower.Contains);
    }

    private static string NormalizeSlotKey(string input)
    {
        var sb = new StringBuilder(input.Length);

        foreach (var ch in input.ToLowerInvariant())
        {
            sb.Append(char.IsLetterOrDigit(ch) || ch == '.' ? ch : '.');
        }

        var raw = sb.ToString();

        while (raw.Contains("..", StringComparison.Ordinal))
        {
            raw = raw.Replace("..", ".", StringComparison.Ordinal);
        }

        return raw.Trim('.');
    }

    private static string NormalizeInline(string input)
    {
        var sb = new StringBuilder(input.Length);

        foreach (var ch in input.ToLowerInvariant())
        {
            sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
        }

        return string.Join(
            ' ',
            sb.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}
