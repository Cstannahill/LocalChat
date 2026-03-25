using System.Net;
using System.Text;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Inference;
using LocalChat.Infrastructure.Inference.HuggingFace;
using LocalChat.Infrastructure.Inference.LlamaCpp;
using LocalChat.Infrastructure.Inference.Ollama;
using LocalChat.Infrastructure.Inference.OpenRouter;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalChat.Infrastructure.Tests;

public sealed class LlamaCppRoutingTests
{
    [Fact]
    public async Task RoutedInferenceProvider_UsesLlamaCpp_ForLlamaCppModelRoute()
    {
        var llamaHandler = new DelegateHttpMessageHandler((request, _) =>
        {
            Assert.Equal("http://localhost:8080/v1/completions", request.RequestUri!.ToString());

            var content = "data: {\"choices\":[{\"text\":\"Hello\"}]}\n\n" +
                          "data: [DONE]\n\n";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/event-stream")
            });
        });

        var throwHandler = new DelegateHttpMessageHandler(static (_, _) =>
            throw new InvalidOperationException("Unexpected provider call."));

        var routed = new RoutedInferenceProvider(
            new OllamaInferenceProvider(new HttpClient(throwHandler), new OllamaOptions()),
            new OpenRouterInferenceProvider(
                new HttpClient(throwHandler),
                new OpenRouterOptions { ApiKey = "test-key", DefaultModel = "unused" },
                NullLogger<OpenRouterInferenceProvider>.Instance),
            new HuggingFaceInferenceProvider(
                new HttpClient(throwHandler),
                new HuggingFaceOptions { ApiKey = "test-key", DefaultModel = "unused" },
                NullLogger<HuggingFaceInferenceProvider>.Instance),
            new LlamaCppInferenceProvider(
                new HttpClient(llamaHandler),
                new LlamaCppOptions { BaseUrl = "http://localhost:8080", DefaultModel = "local-gguf-model" },
                NullLogger<LlamaCppInferenceProvider>.Instance),
            NullLogger<RoutedInferenceProvider>.Instance);

        var deltas = new List<string>();
        var result = await routed.StreamCompletionAsync(
            "prompt",
            (delta, _) =>
            {
                deltas.Add(delta);
                return Task.CompletedTask;
            },
            new InferenceExecutionSettings
            {
                ModelIdentifier = "llamacpp:local-gguf-model"
            });

        Assert.Equal("Hello", result);
        Assert.Single(deltas);
        Assert.Equal("Hello", deltas[0]);
    }

    [Fact]
    public async Task LlamaCppModelContextService_ReadsContextWindowFromProps()
    {
        var handler = new DelegateHttpMessageHandler(static (_, _) =>
        {
            var payload = """
                          {
                            "default_generation_settings": {
                              "n_ctx": 32768
                            }
                          }
                          """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        });

        var service = new LlamaCppModelContextService(
            new HttpClient(handler),
            new LlamaCppOptions
            {
                BaseUrl = "http://localhost:8080",
                DefaultContextWindow = 8192,
                ReservedOutputTokens = 2048,
                SafetyMarginTokens = 512
            },
            NullLogger<LlamaCppModelContextService>.Instance);

        var info = await service.GetForModelAsync("local-gguf-model");

        Assert.Equal(32768, info.EffectiveContextLength);
        Assert.Equal(30208, info.MaxPromptTokens);
    }

    [Fact]
    public async Task RoutedModelContextService_UsesLlamaCppProps_ForLlamaCppRoute()
    {
        var llamaHandler = new DelegateHttpMessageHandler((request, _) =>
        {
            Assert.Contains("/props", request.RequestUri!.ToString(), StringComparison.Ordinal);

            const string payload = """
                                   {
                                     "default_generation_settings": {
                                       "n_ctx": 16384
                                     }
                                   }
                                   """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        });

        var throwHandler = new DelegateHttpMessageHandler(static (_, _) =>
            throw new InvalidOperationException("Unexpected provider context call."));

        var routed = new RoutedModelContextService(
            new OllamaModelContextService(
                new OllamaOptions(),
                new OllamaModelInfoClient(new HttpClient(throwHandler)),
                NullLogger<OllamaModelContextService>.Instance),
            new OpenRouterModelContextService(
                new OpenRouterOptions(),
                NullLogger<OpenRouterModelContextService>.Instance),
            new HuggingFaceModelContextService(
                new HuggingFaceOptions(),
                NullLogger<HuggingFaceModelContextService>.Instance),
            new LlamaCppModelContextService(
                new HttpClient(llamaHandler),
                new LlamaCppOptions
                {
                    BaseUrl = "http://localhost:8080",
                    DefaultContextWindow = 4096
                },
                NullLogger<LlamaCppModelContextService>.Instance),
            NullLogger<RoutedModelContextService>.Instance);

        var info = await routed.GetForModelAsync("llamacpp:local-gguf-model", null);

        Assert.Equal(16384, info.EffectiveContextLength);
    }

    private sealed class DelegateHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request, cancellationToken);
    }
}
