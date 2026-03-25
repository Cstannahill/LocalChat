namespace LocalChat.Application.Abstractions.Prompting;

public sealed class PromptSceneStateSuppressedDebugItem
{
    public required Guid MemoryId { get; init; }

    public required string SlotFamily { get; init; }

    public required string Content { get; init; }

    public required string Reason { get; init; }
}
