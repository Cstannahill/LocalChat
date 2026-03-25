using LocalChat.Application.Prompting.Composition;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Application.Inspection;

public sealed class ContextInspectionResult
{
    public required string Query { get; init; }

    public required string ModelName { get; init; }

    public string? ModelProfileName { get; init; }

    public string? GenerationPresetName { get; init; }

    public required int EffectiveContextLength { get; init; }

    public required int MaxPromptTokens { get; init; }

    public required int EstimatedPromptTokens { get; init; }

    public required bool FitsWithinBudget { get; init; }

    public required string FinalPrompt { get; init; }

    public IReadOnlyList<PromptSection> Sections { get; init; } = Array.Empty<PromptSection>();

    public IReadOnlyList<PromptSessionStateSelectedDebugItem> SelectedSessionState { get; init; } = Array.Empty<PromptSessionStateSelectedDebugItem>();

    public IReadOnlyList<PromptSessionStateSuppressedDebugItem> SuppressedSessionState { get; init; } = Array.Empty<PromptSessionStateSuppressedDebugItem>();

    public IReadOnlyList<PromptDurableMemorySelectedDebugItem> SelectedDurableMemory { get; init; } = Array.Empty<PromptDurableMemorySelectedDebugItem>();

    public IReadOnlyList<PromptDurableMemorySuppressedDebugItem> SuppressedDurableMemory { get; init; } = Array.Empty<PromptDurableMemorySuppressedDebugItem>();

    public IReadOnlyList<MemoryItem> SelectedMemories { get; init; } = Array.Empty<MemoryItem>();

    public IReadOnlyList<LoreEntry> SelectedLoreEntries { get; init; } = Array.Empty<LoreEntry>();

    public string? RollingSummary { get; init; }

    public bool SummaryUsedInPrompt { get; init; }

    public Guid? LatestSummaryCheckpointId { get; init; }

    public DateTime? LatestSummaryCreatedAt { get; init; }

    public int? SummaryStartSequenceNumber { get; init; }

    public int? SummaryEndSequenceNumber { get; init; }

    public int SummaryCoveredMessageCount { get; init; }

    public int TotalPriorMessageCount { get; init; }

    public int IncludedRawMessageCount { get; init; }

    public int ExcludedRawMessageCount { get; init; }
}
