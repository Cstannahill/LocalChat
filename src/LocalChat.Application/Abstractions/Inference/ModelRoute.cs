using LocalChat.Domain.Enums;

namespace LocalChat.Application.Abstractions.Inference;

public static class ModelRoute
{
    public static (ProviderType Provider, string? Model) Parse(
        string? modelIdentifier,
        ProviderType defaultProvider = ProviderType.Ollama)
    {
        if (string.IsNullOrWhiteSpace(modelIdentifier))
        {
            return (defaultProvider, null);
        }

        var trimmed = modelIdentifier.Trim();
        var firstColon = trimmed.IndexOf(':');

        if (firstColon <= 0)
        {
            return (defaultProvider, trimmed);
        }

        var prefix = trimmed[..firstColon].Trim().ToLowerInvariant();
        var model = trimmed[(firstColon + 1)..].Trim();

        return prefix switch
        {
            "ollama" => (ProviderType.Ollama, string.IsNullOrWhiteSpace(model) ? null : model),
            "openrouter" => (ProviderType.OpenRouter, string.IsNullOrWhiteSpace(model) ? null : model),
            "hf" or "huggingface" => (ProviderType.HuggingFace, string.IsNullOrWhiteSpace(model) ? null : model),
            "llamacpp" or "llama.cpp" or "llama-cpp" => (ProviderType.LlamaCpp, string.IsNullOrWhiteSpace(model) ? null : model),
            _ => (defaultProvider, trimmed)
        };
    }

    public static string NormalizeForStorage(
        ProviderType providerType,
        string? modelIdentifier)
    {
        var trimmedModel = modelIdentifier?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedModel))
        {
            return string.Empty;
        }

        var parsed = Parse(trimmedModel, providerType);

        return providerType switch
        {
            ProviderType.OpenRouter => parsed.Provider == ProviderType.OpenRouter && !string.IsNullOrWhiteSpace(parsed.Model)
                ? $"openrouter:{parsed.Model}"
                : parsed.Provider == ProviderType.OpenRouter
                    ? trimmedModel
                : parsed.Provider == ProviderType.Ollama || parsed.Provider == ProviderType.HuggingFace
                    ? trimmedModel
                    : $"openrouter:{trimmedModel}",

            ProviderType.HuggingFace => parsed.Provider == ProviderType.HuggingFace && !string.IsNullOrWhiteSpace(parsed.Model)
                ? $"hf:{parsed.Model}"
                : parsed.Provider == ProviderType.HuggingFace
                    ? trimmedModel
                : parsed.Provider == ProviderType.Ollama || parsed.Provider == ProviderType.OpenRouter
                        ? trimmedModel
                    : $"hf:{trimmedModel}",

            ProviderType.LlamaCpp => parsed.Provider == ProviderType.LlamaCpp && !string.IsNullOrWhiteSpace(parsed.Model)
                ? $"llamacpp:{parsed.Model}"
                : parsed.Provider == ProviderType.LlamaCpp
                    ? trimmedModel
                : parsed.Provider == ProviderType.Ollama || parsed.Provider == ProviderType.OpenRouter || parsed.Provider == ProviderType.HuggingFace
                    ? trimmedModel
                    : $"llamacpp:{trimmedModel}",

            _ => parsed.Provider == ProviderType.Ollama && !string.IsNullOrWhiteSpace(parsed.Model)
                ? $"ollama:{parsed.Model}"
                : parsed.Provider == ProviderType.Ollama
                    ? trimmedModel
                : parsed.Provider == ProviderType.OpenRouter || parsed.Provider == ProviderType.HuggingFace || parsed.Provider == ProviderType.LlamaCpp
                    ? trimmedModel
                    : $"ollama:{trimmedModel}"
        };
    }

    public static string ProviderToWireValue(ProviderType providerType) =>
        providerType switch
        {
            ProviderType.OpenRouter => "openrouter",
            ProviderType.HuggingFace => "huggingface",
            ProviderType.LlamaCpp => "llama.cpp",
            _ => "ollama"
        };

    public static bool TryParseProvider(string? value, out ProviderType providerType)
    {
        providerType = ProviderType.Ollama;

        if (string.IsNullOrWhiteSpace(value))
        {
            providerType = ProviderType.Ollama;
            return true;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "ollama" => Set(out providerType, ProviderType.Ollama),
            "openrouter" => Set(out providerType, ProviderType.OpenRouter),
            "huggingface" or "hf" => Set(out providerType, ProviderType.HuggingFace),
            "llamacpp" or "llama.cpp" or "llama-cpp" => Set(out providerType, ProviderType.LlamaCpp),
            _ => false
        };

        static bool Set(out ProviderType target, ProviderType value)
        {
            target = value;
            return true;
        }
    }
}
