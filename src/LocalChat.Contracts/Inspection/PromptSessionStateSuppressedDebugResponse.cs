namespace LocalChat.Contracts.Inspection;

public sealed class PromptSessionStateSuppressedDebugResponse
{
    public required Guid MemoryId { get; init; }

    public required string SlotFamily { get; init; }

    public required string Content { get; init; }

    public required string Reason { get; init; }
}
