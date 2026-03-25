using LocalChat.Contracts.Agents;

namespace LocalChat.Contracts.ImportExport;

public sealed class AgentPortableModel
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Greeting { get; init; }

    public required string PersonalityDefinition { get; init; }

    public required string Scenario { get; init; }

    public string? DefaultTtsVoice { get; init; }

    public string? DefaultVisualStylePreset { get; init; }

    public string? DefaultVisualPromptPrefix { get; init; }

    public string? DefaultVisualNegativePrompt { get; init; }

    public IReadOnlyList<AgentSampleDialogueRequest> SampleDialogues { get; init; } = Array.Empty<AgentSampleDialogueRequest>();
}
