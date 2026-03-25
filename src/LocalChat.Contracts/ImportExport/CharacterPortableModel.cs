using LocalChat.Contracts.Characters;

namespace LocalChat.Contracts.ImportExport;

public sealed class CharacterPortableModel
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

    public IReadOnlyList<CharacterSampleDialogueRequest> SampleDialogues { get; init; } = Array.Empty<CharacterSampleDialogueRequest>();
}
