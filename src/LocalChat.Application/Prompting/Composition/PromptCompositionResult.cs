using LocalChat.Application.Abstractions.Prompting;

namespace LocalChat.Application.Prompting.Composition;

public sealed class PromptCompositionResult
{
    public required string Prompt { get; init; }

    public required IReadOnlyList<PromptSection> Sections { get; init; }

    public required IReadOnlyList<PromptSessionStateSelectedDebugItem> SelectedSessionState { get; init; }

    public required IReadOnlyList<PromptSessionStateSuppressedDebugItem> SuppressedSessionState { get; init; }

    public required IReadOnlyList<PromptDurableMemorySelectedDebugItem> SelectedDurableMemory { get; init; }

    public required IReadOnlyList<PromptDurableMemorySuppressedDebugItem> SuppressedDurableMemory { get; init; }

    public int EstimatedTokens => Sections.Sum(x => x.EstimatedTokens);
}
