namespace LocalChat.Contracts.Inspection;

public sealed class PromptSessionStateSelectedDebugResponse
{
    public required Guid MemoryId { get; init; }

    public required string SlotFamily { get; init; }

    public string? SlotKey { get; init; }

    public required string Content { get; init; }

    public required string PromptContent { get; init; }
}
