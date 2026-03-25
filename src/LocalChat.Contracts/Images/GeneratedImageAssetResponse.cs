namespace LocalChat.Contracts.Images;

public sealed class GeneratedImageAssetResponse
{
    public required Guid Id { get; init; }

    public required string Url { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required int SortOrder { get; init; }

    public required DateTime CreatedAt { get; init; }
}
