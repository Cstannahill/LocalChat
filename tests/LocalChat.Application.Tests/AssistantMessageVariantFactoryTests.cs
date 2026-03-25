using LocalChat.Application.Chat;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class AssistantMessageVariantFactoryTests
{
    [Fact]
    public void Create_PopulatesProvenanceFields()
    {
        var modelProfileId = Guid.NewGuid();
        var generationPresetId = Guid.NewGuid();

        var provenance = AssistantGenerationProvenance.Create(
            ProviderType.OpenRouter,
            "anthropic/claude-3.7-sonnet",
            modelProfileId,
            generationPresetId);

        var variant = AssistantMessageVariantFactory.Create(
            "Hello there",
            2,
            provenance);

        Assert.Equal("Hello there", variant.Content);
        Assert.Equal(2, variant.VariantIndex);
        Assert.Equal(ProviderType.OpenRouter, variant.ProviderType);
        Assert.Equal("openrouter:anthropic/claude-3.7-sonnet", variant.ModelIdentifier);
        Assert.Equal(modelProfileId, variant.ModelProfileId);
        Assert.Equal(generationPresetId, variant.GenerationPresetId);
    }
}
