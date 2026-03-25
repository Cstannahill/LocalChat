using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using LocalChat.Application.Memory;

namespace LocalChat.Infrastructure.Retrieval;

public static class MemoryCandidateRanker
{
    public static MemoryRankingResult Rank(
        IReadOnlyList<MemoryItem> memories,
        IReadOnlyDictionary<Guid, double> semanticScores,
        Guid? activeConversationId,
        int currentSequenceNumber,
        IMemoryPolicyService memoryPolicyService,
        int maxCount)
    {
        var scored = memories
            .Where(x => x.ReviewStatus == MemoryReviewStatus.Accepted)
            .Where(x => !x.ConversationId.HasValue || !activeConversationId.HasValue || x.ConversationId.Value == activeConversationId.Value || x.ScopeType == MemoryScopeType.Character)
            .Where(x => !activeConversationId.HasValue || !memoryPolicyService.ShouldExcludeFromRetrieval(x, activeConversationId.Value, currentSequenceNumber))
            .Select(x => CreateScoredMemory(x, semanticScores, activeConversationId, currentSequenceNumber, memoryPolicyService))
            .OrderByDescending(x => x.FinalScore)
            .ThenByDescending(x => x.Memory.UpdatedAt)
            .ToList();

        var selectedBuilders = new List<SelectedMemoryBuilder>();
        var selectedBySlot = new Dictionary<string, SelectedMemoryBuilder>(StringComparer.Ordinal);

        SelectLane(
            scored.Where(x => x.Memory.Kind == MemoryKind.SceneState),
            selectedBuilders,
            selectedBySlot,
            maxCount);

        SelectLane(
            scored.Where(x => x.Memory.Kind != MemoryKind.SceneState),
            selectedBuilders,
            selectedBySlot,
            maxCount);

        var selected = selectedBuilders
            .Select(x => new RankedMemorySelection
            {
                Memory = x.Scored.Memory,
                SemanticScore = x.Scored.SemanticScore,
                FinalScore = x.Scored.FinalScore,
                WhySelected = BuildWhySelected(x),
                SuppressedMemories = x.SuppressedMemories
                    .Select(s => new SuppressedRankedMemory
                    {
                        Memory = s.Memory,
                        FinalScore = s.FinalScore,
                        Reason = s.Reason
                    })
                    .ToList()
            })
            .ToList();

        return new MemoryRankingResult
        {
            Selected = selected
        };
    }

    private static void SelectLane(
        IEnumerable<ScoredMemory> candidates,
        List<SelectedMemoryBuilder> selectedBuilders,
        Dictionary<string, SelectedMemoryBuilder> selectedBySlot,
        int maxCount)
    {
        foreach (var candidate in candidates)
        {
            if (selectedBuilders.Count >= maxCount)
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(candidate.Memory.SlotKey) &&
                selectedBySlot.TryGetValue(candidate.Memory.SlotKey, out var existing))
            {
                existing.SuppressedMemories.Add(new SuppressedMemoryBuilder
                {
                    Memory = candidate.Memory,
                    FinalScore = candidate.FinalScore,
                    Reason = $"Suppressed because slot '{candidate.Memory.SlotKey}' was already occupied by a higher-ranked selected memory."
                });

                continue;
            }

            var builder = new SelectedMemoryBuilder
            {
                Scored = candidate
            };

            selectedBuilders.Add(builder);

            if (!string.IsNullOrWhiteSpace(candidate.Memory.SlotKey))
            {
                selectedBySlot[candidate.Memory.SlotKey] = builder;
            }
        }
    }

    private static ScoredMemory CreateScoredMemory(
        MemoryItem memory,
        IReadOnlyDictionary<Guid, double> semanticScores,
        Guid? activeConversationId,
        int currentSequenceNumber,
        IMemoryPolicyService memoryPolicyService)
    {
        var semantic = semanticScores.TryGetValue(memory.Id, out var value) ? value : 0.0;

        var score = semantic;
        var sceneBias = 0.0;
        var conversationBoost = 0.0;
        var pinBoost = 0.0;
        var conflictPenalty = 0.0;

        if (activeConversationId.HasValue)
        {
            score = memoryPolicyService.ApplyRetrievalPolicy(
                memory,
                score,
                activeConversationId.Value,
                currentSequenceNumber);
        }

        if (memory.Kind == MemoryKind.SceneState)
        {
            sceneBias = 0.30;
            score += sceneBias;
        }
        else
        {
            sceneBias = 0.06;
            score += sceneBias;
        }

        if (memory.ConversationId.HasValue &&
            activeConversationId.HasValue &&
            memory.ConversationId.Value == activeConversationId.Value)
        {
            conversationBoost = 0.08;
            score += conversationBoost;
        }

        if (memory.IsPinned)
        {
            pinBoost = 0.08;
            score += pinBoost;
        }

        if (memory.ConflictsWithMemoryItemId.HasValue)
        {
            conflictPenalty = 0.20;
            score -= conflictPenalty;
        }

        return new ScoredMemory
        {
            Memory = memory,
            SemanticScore = semantic,
            FinalScore = score,
            SceneBias = sceneBias,
            ConversationBoost = conversationBoost,
            PinBoost = pinBoost,
            ConflictPenalty = conflictPenalty
        };
    }

    private static string BuildWhySelected(SelectedMemoryBuilder builder)
    {
        var parts = new List<string>
        {
            $"Semantic score: {builder.Scored.SemanticScore:0.000}.",
            $"Final score: {builder.Scored.FinalScore:0.000}."
        };

        if (builder.Scored.Memory.Kind == MemoryKind.SceneState)
        {
            parts.Add("Active scene-state received a priority boost so current temporary context wins over older durable context when they compete.");
        }
        else
        {
            parts.Add("Durable fact remained eligible because no higher-priority scene-state suppressed its slot.");
        }

        if (!string.IsNullOrWhiteSpace(builder.Scored.Memory.SlotKey))
        {
            parts.Add($"Occupies slot '{builder.Scored.Memory.SlotKey}'.");
        }

        if (builder.Scored.ConversationBoost > 0)
        {
            parts.Add("Conversation-scoped boost applied.");
        }

        if (builder.Scored.PinBoost > 0)
        {
            parts.Add("Pinned-memory boost applied.");
        }

        if (builder.SuppressedMemories.Count > 0)
        {
            parts.Add($"Suppressed {builder.SuppressedMemories.Count} lower-ranked memory item(s) in the same slot.");
        }

        return string.Join(" ", parts);
    }

    private sealed class ScoredMemory
    {
        public required MemoryItem Memory { get; init; }

        public required double SemanticScore { get; init; }

        public required double FinalScore { get; init; }

        public required double SceneBias { get; init; }

        public required double ConversationBoost { get; init; }

        public required double PinBoost { get; init; }

        public required double ConflictPenalty { get; init; }
    }

    private sealed class SelectedMemoryBuilder
    {
        public required ScoredMemory Scored { get; init; }

        public List<SuppressedMemoryBuilder> SuppressedMemories { get; } = new();
    }

    private sealed class SuppressedMemoryBuilder
    {
        public required MemoryItem Memory { get; init; }

        public required double FinalScore { get; init; }

        public required string Reason { get; init; }
    }
}

public sealed class MemoryRankingResult
{
    public required IReadOnlyList<RankedMemorySelection> Selected { get; init; }
}

public sealed class RankedMemorySelection
{
    public required MemoryItem Memory { get; init; }

    public required double SemanticScore { get; init; }

    public required double FinalScore { get; init; }

    public required string WhySelected { get; init; }

    public required IReadOnlyList<SuppressedRankedMemory> SuppressedMemories { get; init; }
}

public sealed class SuppressedRankedMemory
{
    public required MemoryItem Memory { get; init; }

    public required double FinalScore { get; init; }

    public required string Reason { get; init; }
}
