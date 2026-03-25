namespace LocalChat.Contracts.Memory;

public sealed class ReviewMemoryRequest
{
    public required string Status { get; init; }

    public bool? IsPinned { get; init; }
}
