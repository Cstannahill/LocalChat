using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class MemoryExtractionDecision
{
    public required MemoryKind Kind { get; init; }

    public required MemoryCategory Category { get; init; }

    public required MemoryReviewStatus ReviewStatus { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public required string Reason { get; init; }
}
