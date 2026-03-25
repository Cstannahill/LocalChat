using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Models;

public sealed class ModelProfile
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ProviderType ProviderType { get; set; } = ProviderType.Ollama;

    public string ModelIdentifier { get; set; } = string.Empty;

    public int? ContextWindow { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
