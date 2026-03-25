using LocalChat.Application.Chat;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class AssistantGenerationProvenanceTests
{
    [Fact]
    public void Create_NormalizesModelIdentifier_ForOllama()
    {
        var result = AssistantGenerationProvenance.Create(
            ProviderType.Ollama,
            "qwen2.5:14b-instruct",
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Equal(ProviderType.Ollama, result.ProviderType);
        Assert.Equal("ollama:qwen2.5:14b-instruct", result.ModelIdentifier);
    }

    [Fact]
    public void Create_NormalizesModelIdentifier_ForOpenRouter()
    {
        var result = AssistantGenerationProvenance.Create(
            ProviderType.OpenRouter,
            "openai/gpt-4.1-mini",
            null,
            null);

        Assert.Equal(ProviderType.OpenRouter, result.ProviderType);
        Assert.Equal("openrouter:openai/gpt-4.1-mini", result.ModelIdentifier);
    }

    [Fact]
    public void Create_NormalizesModelIdentifier_ForHuggingFace()
    {
        var result = AssistantGenerationProvenance.Create(
            ProviderType.HuggingFace,
            "Qwen/Qwen2.5-7B-Instruct",
            null,
            null);

        Assert.Equal(ProviderType.HuggingFace, result.ProviderType);
        Assert.Equal("hf:Qwen/Qwen2.5-7B-Instruct", result.ModelIdentifier);
    }
}
