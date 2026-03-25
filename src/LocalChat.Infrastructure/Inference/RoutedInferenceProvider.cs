using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Inference.HuggingFace;
using LocalChat.Infrastructure.Inference.LlamaCpp;
using LocalChat.Infrastructure.Inference.Ollama;
using LocalChat.Infrastructure.Inference.OpenRouter;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference;

public sealed class RoutedInferenceProvider : IInferenceProvider
{
    private readonly OllamaInferenceProvider _ollamaInferenceProvider;
    private readonly OpenRouterInferenceProvider _openRouterInferenceProvider;
    private readonly HuggingFaceInferenceProvider _huggingFaceInferenceProvider;
    private readonly LlamaCppInferenceProvider _llamaCppInferenceProvider;
    private readonly ILogger<RoutedInferenceProvider> _logger;

    public RoutedInferenceProvider(
        OllamaInferenceProvider ollamaInferenceProvider,
        OpenRouterInferenceProvider openRouterInferenceProvider,
        HuggingFaceInferenceProvider huggingFaceInferenceProvider,
        LlamaCppInferenceProvider llamaCppInferenceProvider,
        ILogger<RoutedInferenceProvider> logger)
    {
        _ollamaInferenceProvider = ollamaInferenceProvider;
        _openRouterInferenceProvider = openRouterInferenceProvider;
        _huggingFaceInferenceProvider = huggingFaceInferenceProvider;
        _llamaCppInferenceProvider = llamaCppInferenceProvider;
        _logger = logger;
    }

    public Task<string> StreamCompletionAsync(
        string prompt,
        Func<string, CancellationToken, Task> onDelta,
        InferenceExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var route = ModelRoute.Parse(executionSettings?.ModelIdentifier, ProviderType.Ollama);

        _logger.LogInformation(
            "Inference route selected. Provider={Provider}, Model={Model}",
            route.Provider,
            route.Model ?? "(provider default)");

        var routedExecutionSettings = executionSettings is null
            ? (route.Model is null ? null : new InferenceExecutionSettings { ModelIdentifier = route.Model })
            : new InferenceExecutionSettings
            {
                ModelIdentifier = route.Model,
                ContextWindow = executionSettings.ContextWindow,
                MaxOutputTokens = executionSettings.MaxOutputTokens,
                Temperature = executionSettings.Temperature,
                TopP = executionSettings.TopP,
                RepeatPenalty = executionSettings.RepeatPenalty,
                StopSequences = executionSettings.StopSequences
            };

        return route.Provider switch
        {
            ProviderType.OpenRouter => _openRouterInferenceProvider.StreamCompletionAsync(
                prompt,
                onDelta,
                routedExecutionSettings,
                cancellationToken),
            ProviderType.HuggingFace => _huggingFaceInferenceProvider.StreamCompletionAsync(
                prompt,
                onDelta,
                routedExecutionSettings,
                cancellationToken),
            ProviderType.LlamaCpp => _llamaCppInferenceProvider.StreamCompletionAsync(
                prompt,
                onDelta,
                routedExecutionSettings,
                cancellationToken),

            _ => _ollamaInferenceProvider.StreamCompletionAsync(
                prompt,
                onDelta,
                routedExecutionSettings,
                cancellationToken)
        };
    }
}
