namespace LocalChat.Contracts.PromptSnapshots;

public sealed class GenerationPromptSnapshotResponse
{
    public required Guid Id { get; init; }

    public required Guid MessageVariantId { get; init; }

    public required Guid MessageId { get; init; }

    public required Guid ConversationId { get; init; }

    public required string FullPromptText { get; init; }

    public required IReadOnlyList<PromptSnapshotSectionResponse> Sections { get; init; }

    public required int EstimatedPromptTokens { get; init; }

    public int? ResolvedContextWindow { get; init; }

    public string? Provider { get; init; }

    public string? ModelIdentifier { get; init; }

    public Guid? ModelProfileId { get; init; }

    public Guid? GenerationPresetId { get; init; }

    public string? RuntimeSource { get; init; }

    public required DateTime CreatedAt { get; init; }

    public string? AssistantCompletion { get; init; }
}
