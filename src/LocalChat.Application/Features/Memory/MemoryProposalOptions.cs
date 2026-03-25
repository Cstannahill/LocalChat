namespace LocalChat.Application.Features.Memory;

public sealed class MemoryProposalOptions
{
    public double MinConfidenceScore { get; set; } = 0.65;

    public int MaxCandidatesPerRun { get; set; } = 10;

    public int MaxRecentMessagesForExtraction { get; set; } = 24;

    public int MinNormalizedKeyLength { get; set; } = 6;

    public bool AutoAcceptDurableFacts { get; set; } = false;

    public double AutoSceneStateMinConfidence { get; set; } = 0.72;

    public double AutoSceneStateMinSceneBound { get; set; } = 0.60;

    public int SceneStateTtlHoursDefault { get; set; } = 8;

    public int SceneStateTtlHoursOutfit { get; set; } = 6;

    public int SceneStateTtlHoursLocation { get; set; } = 8;

    public int SceneStateTtlHoursPoseAction { get; set; } = 4;

    public int SceneStateTtlHoursPossession { get; set; } = 6;

    public int SceneStateTtlHoursEmotionalState { get; set; } = 2;

    public int SceneStateTtlHoursRelationshipState { get; set; } = 10;

    public int SceneStateTtlHoursMisc { get; set; } = 6;

    public double AutoDurableAcceptMinConfidence { get; set; } = 0.98;

    public double AutoDurableAcceptMinExplicitness { get; set; } = 0.92;

    public double AutoDurableAcceptMinPersistence { get; set; } = 0.90;

    public double AutoDurableAcceptMaxSceneBound { get; set; } = 0.20;

    public double AutoDurableAcceptMaxConflictRisk { get; set; } = 0.15;

    public bool EnforceSingleSceneStatePerFamily { get; set; } = true;
}
