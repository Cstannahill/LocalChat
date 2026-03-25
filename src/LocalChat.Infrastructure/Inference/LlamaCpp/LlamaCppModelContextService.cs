using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference.LlamaCpp;

public sealed class LlamaCppModelContextService
{
    private readonly HttpClient _httpClient;
    private readonly LlamaCppOptions _options;
    private readonly ILogger<LlamaCppModelContextService> _logger;

    public LlamaCppModelContextService(
        HttpClient httpClient,
        LlamaCppOptions options,
        ILogger<LlamaCppModelContextService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<ModelContextInfo> GetForModelAsync(
        string? modelIdentifier,
        int? contextWindowOverride = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveContextLength = await ResolveContextLengthAsync(
            modelIdentifier,
            contextWindowOverride,
            cancellationToken);

        var maxPromptTokens = Math.Max(
            512,
            effectiveContextLength - _options.ReservedOutputTokens - _options.SafetyMarginTokens);

        return new ModelContextInfo
        {
            ModelName = modelIdentifier ?? _options.DefaultModel ?? "(default)",
            EffectiveContextLength = effectiveContextLength,
            ReservedOutputTokens = _options.ReservedOutputTokens,
            SafetyMarginTokens = _options.SafetyMarginTokens,
            MaxPromptTokens = maxPromptTokens
        };
    }

    private async Task<int> ResolveContextLengthAsync(
        string? modelIdentifier,
        int? contextWindowOverride,
        CancellationToken cancellationToken)
    {
        if (contextWindowOverride.HasValue && contextWindowOverride.Value > 0)
        {
            return contextWindowOverride.Value;
        }

        if (_options.UsePropsForContext)
        {
            try
            {
                var propsEndpoint = string.IsNullOrWhiteSpace(modelIdentifier)
                    ? $"{_options.BaseUrl.TrimEnd('/')}/props"
                    : $"{_options.BaseUrl.TrimEnd('/')}/props?model={Uri.EscapeDataString(modelIdentifier)}";

                using var response = await _httpClient.GetAsync(propsEndpoint, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                if (doc.RootElement.TryGetProperty("default_generation_settings", out var defaultGenerationSettings) &&
                    defaultGenerationSettings.ValueKind == JsonValueKind.Object &&
                    defaultGenerationSettings.TryGetProperty("n_ctx", out var nCtxProp) &&
                    nCtxProp.ValueKind == JsonValueKind.Number &&
                    nCtxProp.TryGetInt32(out var nCtx) &&
                    nCtx > 0)
                {
                    return nCtx;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to resolve llama.cpp context length from /props for model {Model}. Falling back to default context window.",
                    modelIdentifier ?? "(default)");
            }
        }

        return _options.DefaultContextWindow;
    }
}
