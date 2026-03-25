namespace LocalChat.Contracts.Images;

public sealed class ImageGenerationJobResponse
{
    public required Guid Id { get; init; }

    public required Guid CharacterId { get; init; }

    public required Guid ConversationId { get; init; }

    public required string Provider { get; init; }

    public required string PromptText { get; init; }

    public required string NegativePromptText { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required int Steps { get; init; }

    public required double Cfg { get; init; }

    public required long Seed { get; init; }

    public required string Status { get; init; }

    public string? ErrorMessage { get; init; }

    public string? ProviderJobId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public IReadOnlyList<GeneratedImageAssetResponse> Assets { get; init; } = Array.Empty<GeneratedImageAssetResponse>();
}
