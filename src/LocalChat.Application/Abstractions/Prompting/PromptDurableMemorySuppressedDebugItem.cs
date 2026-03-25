namespace LocalChat.Application.Abstractions.Prompting;

public sealed class PromptDurableMemorySuppressedDebugItem
{
    public required Guid MemoryId { get; init; }

    public required string Category { get; init; }

    public required string Content { get; init; }

    public required string Reason { get; init; }
}
