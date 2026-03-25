using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Generation;

public sealed class GenerationPromptSnapshot
{
    public Guid Id { get; set; }

    public Guid MessageVariantId { get; set; }

    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public string FullPromptText { get; set; } = string.Empty;

    public string PromptSectionsJson { get; set; } = "[]";

    public int EstimatedPromptTokens { get; set; }

    public int? ResolvedContextWindow { get; set; }

    public ProviderType? ProviderType { get; set; }

    public string? ModelIdentifier { get; set; }

    public Guid? ModelProfileId { get; set; }

    public Guid? GenerationPresetId { get; set; }

    public RuntimeSourceType? RuntimeSourceType { get; set; }

    public DateTime CreatedAt { get; set; }
}
