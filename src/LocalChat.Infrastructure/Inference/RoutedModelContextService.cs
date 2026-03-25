using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Inference.HuggingFace;
using LocalChat.Infrastructure.Inference.LlamaCpp;
using LocalChat.Infrastructure.Inference.Ollama;
using LocalChat.Infrastructure.Inference.OpenRouter;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference;

public sealed class RoutedModelContextService : IModelContextService
{
    private readonly OllamaModelContextService _ollamaModelContextService;
    private readonly OpenRouterModelContextService _openRouterModelContextService;
    private readonly HuggingFaceModelContextService _huggingFaceModelContextService;
    private readonly LlamaCppModelContextService _llamaCppModelContextService;
    private readonly ILogger<RoutedModelContextService> _logger;

    public RoutedModelContextService(
        OllamaModelContextService ollamaModelContextService,
        OpenRouterModelContextService openRouterModelContextService,
        HuggingFaceModelContextService huggingFaceModelContextService,
        LlamaCppModelContextService llamaCppModelContextService,
        ILogger<RoutedModelContextService> logger)
    {
        _ollamaModelContextService = ollamaModelContextService;
        _openRouterModelContextService = openRouterModelContextService;
        _huggingFaceModelContextService = huggingFaceModelContextService;
        _llamaCppModelContextService = llamaCppModelContextService;
        _logger = logger;
    }

    public Task<ModelContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return _ollamaModelContextService.GetCurrentAsync(cancellationToken);
    }

    public Task<ModelContextInfo> GetForModelAsync(
        string? modelIdentifier,
        int? contextWindowOverride,
        CancellationToken cancellationToken = default)
    {
        var route = ModelRoute.Parse(modelIdentifier, ProviderType.Ollama);

        _logger.LogInformation(
            "Model context route selected. Provider={Provider}, Model={Model}",
            route.Provider,
            route.Model ?? "(provider default)");

        return route.Provider switch
        {
            ProviderType.OpenRouter => _openRouterModelContextService.GetForModelAsync(
                route.Model,
                contextWindowOverride,
                cancellationToken),
            ProviderType.HuggingFace => _huggingFaceModelContextService.GetForModelAsync(
                route.Model,
                contextWindowOverride,
                cancellationToken),
            ProviderType.LlamaCpp => _llamaCppModelContextService.GetForModelAsync(
                route.Model,
                contextWindowOverride,
                cancellationToken),

            _ => _ollamaModelContextService.GetForModelAsync(
                route.Model ?? modelIdentifier,
                contextWindowOverride,
                cancellationToken)
        };
    }
}
