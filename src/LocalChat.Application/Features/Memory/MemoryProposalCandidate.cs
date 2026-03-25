using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class MemoryProposalCandidate
{
    public required MemoryCategory Category { get; init; }

    public required string Content { get; init; }

    public required double Confidence { get; init; }

    public string? Reason { get; init; }

    public string? Evidence { get; init; }
}
