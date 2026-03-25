namespace LocalChat.Contracts.Models;

public sealed class UpdateModelProfileRequest
{
    public required string Name { get; init; }

    public required string ProviderType { get; init; }

    public required string ModelIdentifier { get; init; }

    public int? ContextWindow { get; init; }

    public required string Notes { get; init; }
}
