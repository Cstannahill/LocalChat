namespace LocalChat.Application.Prompting.Composition;

public sealed class PromptSection
{
    public required string Name { get; init; }

    public required string Content { get; init; }

    public required int EstimatedTokens { get; init; }
}
