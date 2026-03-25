using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Application.Chat;

public static class AssistantMessageVariantFactory
{
    public static MessageVariant Create(
        string content,
        int variantIndex,
        AssistantGenerationProvenance provenance,
        DateTime? generationStartedAt = null,
        DateTime? generationCompletedAt = null)
    {
        var responseTimeMs =
            generationStartedAt.HasValue && generationCompletedAt.HasValue
                ? (int?)Math.Max(
                    0,
                    (int)(generationCompletedAt.Value - generationStartedAt.Value)
                    .TotalMilliseconds)
                : null;

        return new MessageVariant
        {
            Id = Guid.NewGuid(),
            Content = content,
            VariantIndex = variantIndex,
            CreatedAt = DateTime.UtcNow,
            ProviderType = provenance.ProviderType,
            ModelIdentifier = provenance.ModelIdentifier,
            ModelProfileId = provenance.ModelProfileId,
            GenerationPresetId = provenance.GenerationPresetId,
            RuntimeSourceType = provenance.RuntimeSourceType,
            GenerationStartedAt = generationStartedAt,
            GenerationCompletedAt = generationCompletedAt,
            ResponseTimeMs = responseTimeMs
        };
    }
}
