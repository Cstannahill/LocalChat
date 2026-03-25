namespace LocalChat.Application.Abstractions.Inference;

public sealed class ModelContextInfo
{
    public required string ModelName { get; init; }

    public required int EffectiveContextLength { get; init; }

    public required int ReservedOutputTokens { get; init; }

    public required int SafetyMarginTokens { get; init; }

    public required int MaxPromptTokens { get; init; }
}
