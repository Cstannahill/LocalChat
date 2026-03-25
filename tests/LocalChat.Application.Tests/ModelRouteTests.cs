using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class ModelRouteTests
{
    [Fact]
    public void Parse_UnprefixedModel_DefaultsToOllama()
    {
        var result = ModelRoute.Parse("qwen2.5:14b-instruct");

        Assert.Equal(ProviderType.Ollama, result.Provider);
        Assert.Equal("qwen2.5:14b-instruct", result.Model);
    }

    [Fact]
    public void Parse_OpenRouterPrefix_RoutesToOpenRouter()
    {
        var result = ModelRoute.Parse("openrouter:anthropic/claude-3.7-sonnet");

        Assert.Equal(ProviderType.OpenRouter, result.Provider);
        Assert.Equal("anthropic/claude-3.7-sonnet", result.Model);
    }

    [Fact]
    public void Parse_OllamaPrefix_RoutesToOllama()
    {
        var result = ModelRoute.Parse("ollama:qwen2.5:14b-instruct");

        Assert.Equal(ProviderType.Ollama, result.Provider);
        Assert.Equal("qwen2.5:14b-instruct", result.Model);
    }

    [Fact]
    public void Parse_HfPrefix_RoutesToHuggingFace()
    {
        var result = ModelRoute.Parse("hf:Qwen/Qwen2.5-7B-Instruct");

        Assert.Equal(ProviderType.HuggingFace, result.Provider);
        Assert.Equal("Qwen/Qwen2.5-7B-Instruct", result.Model);
    }

    [Fact]
    public void Parse_HuggingFacePrefix_RoutesToHuggingFace()
    {
        var result = ModelRoute.Parse("huggingface:meta-llama/Llama-3.1-8B-Instruct");

        Assert.Equal(ProviderType.HuggingFace, result.Provider);
        Assert.Equal("meta-llama/Llama-3.1-8B-Instruct", result.Model);
    }

    [Fact]
    public void Parse_LlamaCppPrefix_RoutesToLlamaCpp()
    {
        var result = ModelRoute.Parse("llamacpp:local-gguf-model");

        Assert.Equal(ProviderType.LlamaCpp, result.Provider);
        Assert.Equal("local-gguf-model", result.Model);
    }

    [Fact]
    public void Parse_LlamaCppAliasPrefixes_RouteToLlamaCpp()
    {
        var dotted = ModelRoute.Parse("llama.cpp:local-gguf-model");
        var dashed = ModelRoute.Parse("llama-cpp:local-gguf-model");

        Assert.Equal(ProviderType.LlamaCpp, dotted.Provider);
        Assert.Equal("local-gguf-model", dotted.Model);
        Assert.Equal(ProviderType.LlamaCpp, dashed.Provider);
        Assert.Equal("local-gguf-model", dashed.Model);
    }

    [Fact]
    public void NormalizeForStorage_OpenRouter_AddsPrefix()
    {
        var result = ModelRoute.NormalizeForStorage(
            ProviderType.OpenRouter,
            "anthropic/claude-3.7-sonnet");

        Assert.Equal("openrouter:anthropic/claude-3.7-sonnet", result);
    }

    [Fact]
    public void NormalizeForStorage_Hf_AddsPrefix()
    {
        var result = ModelRoute.NormalizeForStorage(
            ProviderType.HuggingFace,
            "Qwen/Qwen2.5-7B-Instruct");

        Assert.Equal("hf:Qwen/Qwen2.5-7B-Instruct", result);
    }

    [Fact]
    public void NormalizeForStorage_Ollama_AddsPrefix()
    {
        var result = ModelRoute.NormalizeForStorage(
            ProviderType.Ollama,
            "qwen2.5:14b-instruct");

        Assert.Equal("ollama:qwen2.5:14b-instruct", result);
    }

    [Fact]
    public void NormalizeForStorage_LlamaCpp_AddsCanonicalPrefix()
    {
        var result = ModelRoute.NormalizeForStorage(
            ProviderType.LlamaCpp,
            "llama.cpp:local-gguf-model");

        Assert.Equal("llamacpp:local-gguf-model", result);
    }

    [Fact]
    public void NormalizeForStorage_Ollama_LeavesOpenRouterPrefixedValueAlone()
    {
        var result = ModelRoute.NormalizeForStorage(
            ProviderType.Ollama,
            "openrouter:openai/gpt-4.1-mini");

        Assert.Equal("openrouter:openai/gpt-4.1-mini", result);
    }

    [Fact]
    public void TryParseProvider_ParsesOpenRouter()
    {
        var ok = ModelRoute.TryParseProvider("openrouter", out var provider);

        Assert.True(ok);
        Assert.Equal(ProviderType.OpenRouter, provider);
    }

    [Fact]
    public void TryParseProvider_ParsesHuggingFace()
    {
        var ok = ModelRoute.TryParseProvider("huggingface", out var provider);

        Assert.True(ok);
        Assert.Equal(ProviderType.HuggingFace, provider);
    }

    [Fact]
    public void TryParseProvider_ParsesHfAlias()
    {
        var ok = ModelRoute.TryParseProvider("hf", out var provider);

        Assert.True(ok);
        Assert.Equal(ProviderType.HuggingFace, provider);
    }

    [Fact]
    public void TryParseProvider_ParsesLlamaCppAliases()
    {
        var canonical = ModelRoute.TryParseProvider("llama.cpp", out var canonicalProvider);
        var compact = ModelRoute.TryParseProvider("llamacpp", out var compactProvider);
        var dashed = ModelRoute.TryParseProvider("llama-cpp", out var dashedProvider);

        Assert.True(canonical);
        Assert.Equal(ProviderType.LlamaCpp, canonicalProvider);
        Assert.True(compact);
        Assert.Equal(ProviderType.LlamaCpp, compactProvider);
        Assert.True(dashed);
        Assert.Equal(ProviderType.LlamaCpp, dashedProvider);
    }
}
