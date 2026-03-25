namespace LocalChat.Contracts.Inspection;

public sealed class PromptInspectionResponse
{
    public required string Query { get; init; }

    public required string ModelName { get; init; }

    public string? ModelProfileName { get; init; }

    public string? GenerationPresetName { get; init; }

    public required int EffectiveContextLength { get; init; }

    public required int MaxPromptTokens { get; init; }

    public required int EstimatedPromptTokens { get; init; }

    public required bool FitsWithinBudget { get; init; }

    public required string FinalPrompt { get; init; }

    public string? CharacterDefinitionSection { get; init; }

    public string? CharacterScenarioSection { get; init; }

    public string? SampleDialogueSection { get; init; }

    public string? UserPersonaSection { get; init; }

    public string? DirectorSection { get; init; }

    public string? SceneContextSection { get; init; }

    public string? OocModeSection { get; init; }

    public IReadOnlyList<PromptSectionResponse> Sections { get; init; } = Array.Empty<PromptSectionResponse>();

    public required IReadOnlyList<PromptSceneStateSelectedDebugResponse> SelectedSceneState { get; init; }

    public required IReadOnlyList<PromptSceneStateSuppressedDebugResponse> SuppressedSceneState { get; init; }

    public required IReadOnlyList<PromptDurableMemorySelectedDebugResponse> SelectedDurableMemory { get; init; }

    public required IReadOnlyList<PromptDurableMemorySuppressedDebugResponse> SuppressedDurableMemory { get; init; }
}

public sealed class PromptSectionResponse
{
    public required string Name { get; init; }

    public required string Content { get; init; }

    public required int EstimatedTokens { get; init; }
}
