using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class MemoryExtractionClassifier
{
    private readonly MemoryProposalOptions _options;

    public MemoryExtractionClassifier(MemoryProposalOptions options)
    {
        _options = options;
    }

    public MemoryExtractionDecision Classify(
        ExtractedMemoryCandidate candidate,
        bool hasConflict)
    {
        if (candidate.Category == MemoryCategory.SceneState)
        {
            var isSceneState =
                candidate.ConfidenceScore >= _options.AutoSceneStateMinConfidence &&
                candidate.SceneBoundScore >= _options.AutoSceneStateMinSceneBound;

            if (isSceneState)
            {
                return new MemoryExtractionDecision
                {
                    Category = MemoryCategory.SceneState,
                    Kind = MemoryKind.SceneState,
                    ReviewStatus = MemoryReviewStatus.Accepted,
                    ExpiresAt = null,
                    Reason = "Scene-bound state was auto-saved."
                };
            }
        }

        var canAutoAcceptDurable =
            _options.AutoAcceptDurableFacts &&
            !hasConflict &&
            candidate.ConfidenceScore >= _options.AutoDurableAcceptMinConfidence &&
            candidate.ExplicitnessScore >= _options.AutoDurableAcceptMinExplicitness &&
            candidate.PersistenceScore >= _options.AutoDurableAcceptMinPersistence &&
            candidate.SceneBoundScore <= _options.AutoDurableAcceptMaxSceneBound &&
            candidate.ConflictRiskScore <= _options.AutoDurableAcceptMaxConflictRisk;

        return new MemoryExtractionDecision
        {
            Category = candidate.Category,
            Kind = MemoryKind.DurableFact,
            ReviewStatus = canAutoAcceptDurable
                ? MemoryReviewStatus.Accepted
                : MemoryReviewStatus.Proposed,
            ExpiresAt = null,
            Reason = canAutoAcceptDurable
                ? "High-confidence explicit durable fact auto-accepted."
                : "Candidate stored as a durable-memory proposal pending review."
        };
    }
}
