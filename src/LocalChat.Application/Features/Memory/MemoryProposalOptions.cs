namespace LocalChat.Application.Features.Memory;

public sealed class MemoryProposalOptions
{
    public double MinConfidenceScore { get; set; } = 0.65;

    public int MaxCandidatesPerRun { get; set; } = 10;

    public int MaxRecentMessagesForExtraction { get; set; } = 24;

    public int MinNormalizedKeyLength { get; set; } = 6;

    public bool AutoAcceptDurableFacts { get; set; } = false;

    public double AutoSessionStateMinConfidence { get; set; } = 0.72;

    public double AutoSessionStateMinSceneBound { get; set; } = 0.60;

    public int SessionStateTtlHoursDefault { get; set; } = 8;

    public int SessionStateTtlHoursOutfit { get; set; } = 6;

    public int SessionStateTtlHoursLocation { get; set; } = 8;

    public int SessionStateTtlHoursPoseAction { get; set; } = 4;

    public int SessionStateTtlHoursPossession { get; set; } = 6;

    public int SessionStateTtlHoursEmotionalState { get; set; } = 2;

    public int SessionStateTtlHoursRelationshipState { get; set; } = 10;

    public int SessionStateTtlHoursMisc { get; set; } = 6;

    public double AutoDurableAcceptMinConfidence { get; set; } = 0.98;

    public double AutoDurableAcceptMinExplicitness { get; set; } = 0.92;

    public double AutoDurableAcceptMinPersistence { get; set; } = 0.90;

    public double AutoDurableAcceptMaxSceneBound { get; set; } = 0.20;

    public double AutoDurableAcceptMaxConflictRisk { get; set; } = 0.15;

    public bool EnforceSingleSessionStatePerFamily { get; set; } = true;
}
