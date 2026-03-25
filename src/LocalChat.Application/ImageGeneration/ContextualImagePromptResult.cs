namespace LocalChat.Application.ImageGeneration;

public sealed class ContextualImagePromptResult
{
    public required Guid ConversationId { get; init; }

    public required string PositivePrompt { get; init; }

    public required string NegativePrompt { get; init; }

    public string? SceneSummary { get; init; }

    public IReadOnlyList<string> AssumptionsOrUnknowns { get; init; } = Array.Empty<string>();
}
