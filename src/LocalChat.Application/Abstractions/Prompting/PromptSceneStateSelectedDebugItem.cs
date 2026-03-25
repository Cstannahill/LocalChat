namespace LocalChat.Application.Abstractions.Prompting;

public sealed class PromptSceneStateSelectedDebugItem
{
    public required Guid MemoryId { get; init; }

    public required string SlotFamily { get; init; }

    public string? SlotKey { get; init; }

    public required string Content { get; init; }

    public required string PromptContent { get; init; }
}
