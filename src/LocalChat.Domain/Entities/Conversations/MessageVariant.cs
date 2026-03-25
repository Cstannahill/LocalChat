using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Conversations;

public sealed class MessageVariant
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public int VariantIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ProviderType? ProviderType { get; set; }

    public string? ModelIdentifier { get; set; }

    public Guid? ModelProfileId { get; set; }

    public Guid? GenerationPresetId { get; set; }

    public RuntimeSourceType? RuntimeSourceType { get; set; }

    public DateTime? GenerationStartedAt { get; set; }

    public DateTime? GenerationCompletedAt { get; set; }

    public int? ResponseTimeMs { get; set; }

    public Message? Message { get; set; }
}
