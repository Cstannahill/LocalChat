namespace LocalChat.Contracts.Images;

public sealed class ContextualImagePromptResponse
{
    public required Guid ConversationId { get; init; }

    public required string PositivePrompt { get; init; }

    public required string NegativePrompt { get; init; }

    public string? SceneSummary { get; init; }

    public IReadOnlyList<string> AssumptionsOrUnknowns { get; init; } = Array.Empty<string>();
}
