using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaModelContextService : IModelContextService
{
    private readonly OllamaOptions _options;
    private readonly OllamaModelInfoClient _modelInfoClient;
    private readonly ILogger<OllamaModelContextService> _logger;

    public OllamaModelContextService(
        OllamaOptions options,
        OllamaModelInfoClient modelInfoClient,
        ILogger<OllamaModelContextService> logger)
    {
        _options = options;
        _modelInfoClient = modelInfoClient;
        _logger = logger;
    }

    public Task<ModelContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return GetForModelAsync(_options.Model, null, cancellationToken);
    }

    public async Task<ModelContextInfo> GetForModelAsync(
        string? modelIdentifier,
        int? contextWindowOverride,
        CancellationToken cancellationToken = default)
    {
        var parsed = ModelRoute.Parse(modelIdentifier, ProviderType.Ollama);
        var modelName = string.IsNullOrWhiteSpace(parsed.Model)
            ? _options.Model
            : parsed.Model;

        int effectiveContextLength;
        if (contextWindowOverride.HasValue && contextWindowOverride.Value > 0)
        {
            effectiveContextLength = contextWindowOverride.Value;
        }
        else
        {
            try
            {
                effectiveContextLength =
                    await _modelInfoClient.GetContextWindowAsync(modelName, cancellationToken)
                    ?? _options.MaxContextFallback;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to resolve Ollama context window for model {Model}. Falling back to default.",
                    modelName);
                effectiveContextLength = _options.MaxContextFallback;
            }
        }

        var maxPromptTokens = Math.Max(
            512,
            effectiveContextLength - _options.ReservedOutputTokens - _options.SafetyMarginTokens);

        return new ModelContextInfo
        {
            ModelName = modelName,
            EffectiveContextLength = effectiveContextLength,
            ReservedOutputTokens = _options.ReservedOutputTokens,
            SafetyMarginTokens = _options.SafetyMarginTokens,
            MaxPromptTokens = maxPromptTokens
        };
    }
}
