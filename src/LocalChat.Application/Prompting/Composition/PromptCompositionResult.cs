using LocalChat.Application.Abstractions.Prompting;

namespace LocalChat.Application.Prompting.Composition;

public sealed class PromptCompositionResult
{
    public required string Prompt { get; init; }

    public required IReadOnlyList<PromptSection> Sections { get; init; }

    public required IReadOnlyList<PromptSceneStateSelectedDebugItem> SelectedSceneState { get; init; }

    public required IReadOnlyList<PromptSceneStateSuppressedDebugItem> SuppressedSceneState { get; init; }

    public required IReadOnlyList<PromptDurableMemorySelectedDebugItem> SelectedDurableMemory { get; init; }

    public required IReadOnlyList<PromptDurableMemorySuppressedDebugItem> SuppressedDurableMemory { get; init; }

    public int EstimatedTokens => Sections.Sum(x => x.EstimatedTokens);
}
