using System.Net.Http.Json;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaInferenceProvider : IInferenceProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaInferenceProvider(HttpClient httpClient, OllamaOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string> StreamCompletionAsync(
        string prompt,
        Func<string, CancellationToken, Task> onDelta,
        InferenceExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest
        {
            Model = executionSettings?.ModelIdentifier ?? _options.Model,
            Prompt = prompt,
            Stream = true,
            KeepAlive = _options.KeepAlive,
            Options = new OllamaGenerateOptions
            {
                NumCtx = executionSettings?.ContextWindow,
                NumPredict = executionSettings?.MaxOutputTokens ?? _options.ReservedOutputTokens,
                Temperature = executionSettings?.Temperature,
                TopP = executionSettings?.TopP,
                RepeatPenalty = executionSettings?.RepeatPenalty,
                Stop = executionSettings?.StopSequences?.Count > 0
                    ? executionSettings.StopSequences
                    : null
            }
        };

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "generate")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Ollama generate request failed with status {(int)response.StatusCode}: {errorBody}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var final = new StringWriter();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var chunk = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line);
            if (chunk is null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(chunk.Response))
            {
                final.Write(chunk.Response);
                await onDelta(chunk.Response, cancellationToken);
            }

            if (chunk.Done)
            {
                break;
            }
        }

        return final.ToString();
    }
}
