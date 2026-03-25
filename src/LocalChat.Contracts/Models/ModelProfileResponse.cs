namespace LocalChat.Contracts.Models;

public sealed class ModelProfileResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string ProviderType { get; init; }

    public required string ModelIdentifier { get; init; }

    public int? ContextWindow { get; init; }

    public required string Notes { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }
}
