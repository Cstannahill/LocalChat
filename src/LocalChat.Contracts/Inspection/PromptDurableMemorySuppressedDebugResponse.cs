namespace LocalChat.Contracts.Inspection;

public sealed class PromptDurableMemorySuppressedDebugResponse
{
    public required Guid MemoryId { get; init; }

    public required string Category { get; init; }

    public required string Content { get; init; }

    public required string Reason { get; init; }
}
