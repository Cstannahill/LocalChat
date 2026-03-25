namespace LocalChat.Contracts.Inspection;

public sealed class PromptDurableMemorySelectedDebugResponse
{
    public required Guid MemoryId { get; init; }

    public required string Category { get; init; }

    public required string Content { get; init; }

    public required string PromptContent { get; init; }
}
