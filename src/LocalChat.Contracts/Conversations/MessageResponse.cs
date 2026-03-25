namespace LocalChat.Contracts.Conversations;

public sealed class MessageResponse
{
    public required Guid Id { get; init; }

    public required string Role { get; init; }

    public required string OriginType { get; init; }

    public required string Content { get; init; }

    public required int SequenceNumber { get; init; }

    public required DateTime CreatedAt { get; init; }

    public int? SelectedVariantIndex { get; init; }

    public int VariantCount { get; init; }

    public IReadOnlyList<MessageVariantResponse> Variants { get; init; } =
        Array.Empty<MessageVariantResponse>();

    public string? SelectedProvider { get; init; }

    public string? SelectedModelIdentifier { get; init; }

    public Guid? SelectedModelProfileId { get; init; }

    public Guid? SelectedGenerationPresetId { get; init; }

    public string? SelectedRuntimeSource { get; init; }

    public DateTime? SelectedGenerationStartedAt { get; init; }

    public DateTime? SelectedGenerationCompletedAt { get; init; }

    public int? SelectedResponseTimeMs { get; init; }
}
