using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Chat;

public sealed class AssistantGenerationProvenance
{
    public required ProviderType ProviderType { get; init; }

    public string? ModelIdentifier { get; init; }

    public Guid? ModelProfileId { get; init; }

    public Guid? GenerationPresetId { get; init; }

    public required RuntimeSourceType RuntimeSourceType { get; init; }

    public static AssistantGenerationProvenance Create(
        ProviderType? providerType,
        string? modelIdentifier,
        Guid? modelProfileId,
        Guid? generationPresetId,
        RuntimeSourceType runtimeSourceType)
    {
        var resolvedProvider = providerType
            ?? ModelRoute.Parse(modelIdentifier, ProviderType.Ollama).Provider;

        var normalizedModelIdentifier = string.IsNullOrWhiteSpace(modelIdentifier)
            ? null
            : ModelRoute.NormalizeForStorage(resolvedProvider, modelIdentifier);

        return new AssistantGenerationProvenance
        {
            ProviderType = resolvedProvider,
            ModelIdentifier = normalizedModelIdentifier,
            ModelProfileId = modelProfileId,
            GenerationPresetId = generationPresetId,
            RuntimeSourceType = runtimeSourceType
        };
    }

    public static AssistantGenerationProvenance Create(
        ProviderType? providerType,
        string? modelIdentifier,
        Guid? modelProfileId,
        Guid? generationPresetId)
    {
        return Create(
            providerType,
            modelIdentifier,
            modelProfileId,
            generationPresetId,
            RuntimeSourceType.ProviderDefault);
    }
}
