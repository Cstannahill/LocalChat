using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class ExtractedMemoryCandidate
{
    public required MemoryCategory Category { get; init; }

    public required string Content { get; init; }

    public required double ConfidenceScore { get; init; }

    public string? ProposalReason { get; init; }

    public string? SourceExcerpt { get; init; }

    public double ExplicitnessScore { get; init; }

    public double PersistenceScore { get; init; }

    public double SceneBoundScore { get; init; }

    public double ConflictRiskScore { get; init; }

    public required string NormalizedKey { get; init; }

    public string? SlotKey { get; init; }

    public MemorySlotFamily SlotFamily { get; init; } = MemorySlotFamily.None;
}
