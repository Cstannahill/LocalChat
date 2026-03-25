namespace LocalChat.Contracts.Memory;

public sealed class MemoryConflictResolutionResponse
{
    public required Guid ProposalMemoryId { get; init; }

    public required string Action { get; init; }

    public required bool Succeeded { get; init; }

    public string? Message { get; init; }

    public Guid? ConflictingMemoryId { get; init; }

    public bool ConflictingMemoryRejected { get; init; }

    public bool RetrievalReindexed { get; init; }
}