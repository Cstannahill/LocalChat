using LocalChat.Application.Features.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class MemoryExtractionClassifierTests
{
    [Fact]
    public void Classify_HighSceneBoundCandidate_BecomesAcceptedSceneState()
    {
        var classifier = new MemoryExtractionClassifier(new MemoryProposalOptions());

        var candidate = new ExtractedMemoryCandidate
        {
            Category = MemoryCategory.SceneState,
            Content = "The character is currently wearing a yellow sundress.",
            ConfidenceScore = 0.90,
            ProposalReason = "Current visible outfit.",
            SourceExcerpt = "You look beautiful in that yellow sundress.",
            ExplicitnessScore = 0.95,
            PersistenceScore = 0.20,
            SceneBoundScore = 0.95,
            ConflictRiskScore = 0.05,
            NormalizedKey = "SceneState:yellow sundress"
        };

        var decision = classifier.Classify(candidate, hasConflict: false);

        Assert.Equal(MemoryKind.SceneState, decision.Kind);
        Assert.Equal(MemoryReviewStatus.Accepted, decision.ReviewStatus);
        Assert.Equal(MemoryCategory.SceneState, decision.Category);
        Assert.Null(decision.ExpiresAt);
    }

    [Fact]
    public void Classify_DurableCandidate_WithAutoAcceptDisabled_BecomesProposal()
    {
        var classifier = new MemoryExtractionClassifier(new MemoryProposalOptions
        {
            AutoAcceptDurableFacts = false
        });

        var candidate = new ExtractedMemoryCandidate
        {
            Category = MemoryCategory.UserFact,
            Content = "The user prefers TypeScript over JavaScript.",
            ConfidenceScore = 0.99,
            ProposalReason = "Directly stated preference.",
            SourceExcerpt = "I always prefer TypeScript over JavaScript.",
            ExplicitnessScore = 0.99,
            PersistenceScore = 0.95,
            SceneBoundScore = 0.05,
            ConflictRiskScore = 0.01,
            NormalizedKey = "UserFact:user prefers typescript"
        };

        var decision = classifier.Classify(candidate, hasConflict: false);

        Assert.Equal(MemoryKind.DurableFact, decision.Kind);
        Assert.Equal(MemoryReviewStatus.Proposed, decision.ReviewStatus);
        Assert.Null(decision.ExpiresAt);
    }
}
