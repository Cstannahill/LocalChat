using LocalChat.Application.Abstractions.Inference;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference.OpenRouter;

public sealed class OpenRouterModelContextService
{
    private const int DefaultReservedOutputTokens = 4096;
    private const int DefaultSafetyMarginTokens = 1024;

    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterModelContextService> _logger;

    public OpenRouterModelContextService(
        OpenRouterOptions options,
        ILogger<OpenRouterModelContextService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<ModelContextInfo> GetForModelAsync(
        string? modelIdentifier,
        int? contextWindowOverride = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveContextLength = contextWindowOverride.HasValue && contextWindowOverride.Value > 0
            ? contextWindowOverride.Value
            : _options.DefaultContextWindow;

        _logger.LogInformation(
            "Using fallback OpenRouter context window for model {Model}: {ContextWindow}",
            modelIdentifier ?? "(default)",
            effectiveContextLength);

        var maxPromptTokens = Math.Max(
            512,
            effectiveContextLength - DefaultReservedOutputTokens - DefaultSafetyMarginTokens);

        return Task.FromResult(new ModelContextInfo
        {
            ModelName = modelIdentifier ?? "(default)",
            EffectiveContextLength = effectiveContextLength,
            ReservedOutputTokens = DefaultReservedOutputTokens,
            SafetyMarginTokens = DefaultSafetyMarginTokens,
            MaxPromptTokens = maxPromptTokens
        });
    }
}
