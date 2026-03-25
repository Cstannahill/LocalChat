namespace LocalChat.Contracts.Conversations;

public sealed class MessageVariantResponse
{
    public required Guid Id { get; init; }

    public required int VariantIndex { get; init; }

    public required string Content { get; init; }

    public required DateTime CreatedAt { get; init; }

    public string? Provider { get; init; }

    public string? ModelIdentifier { get; init; }

    public Guid? ModelProfileId { get; init; }

    public Guid? GenerationPresetId { get; init; }

    public string? RuntimeSource { get; init; }

    public DateTime? GenerationStartedAt { get; init; }

    public DateTime? GenerationCompletedAt { get; init; }

    public int? ResponseTimeMs { get; init; }
}
