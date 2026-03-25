using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference.LlamaCpp;

public sealed class LlamaCppInferenceProvider
{
    private readonly HttpClient _httpClient;
    private readonly LlamaCppOptions _options;
    private readonly ILogger<LlamaCppInferenceProvider> _logger;

    public LlamaCppInferenceProvider(
        HttpClient httpClient,
        LlamaCppOptions options,
        ILogger<LlamaCppInferenceProvider> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<string> StreamCompletionAsync(
        string prompt,
        Func<string, CancellationToken, Task> onDelta,
        InferenceExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var model = !string.IsNullOrWhiteSpace(executionSettings?.ModelIdentifier)
            ? executionSettings!.ModelIdentifier
            : _options.DefaultModel;

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/v1/completions";

        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["prompt"] = prompt,
            ["stream"] = true
        };

        if (executionSettings?.MaxOutputTokens is int maxOutputTokens)
        {
            requestBody["max_tokens"] = maxOutputTokens;
        }

        if (executionSettings?.Temperature is double temperature)
        {
            requestBody["temperature"] = temperature;
        }

        if (executionSettings?.TopP is double topP)
        {
            requestBody["top_p"] = topP;
        }

        if (executionSettings?.StopSequences?.Count > 0)
        {
            requestBody["stop"] = executionSettings.StopSequences;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

        var apiKey = string.IsNullOrWhiteSpace(_options.ApiKey)
            ? "no-key"
            : _options.ApiKey;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        return await StreamResponseAsync(request, onDelta, cancellationToken);
    }

    private async Task<string> StreamResponseAsync(
        HttpRequestMessage request,
        Func<string, CancellationToken, Task> onDelta,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"llama.cpp request failed with status {(int)response.StatusCode}: {errorBody}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var full = new StringBuilder();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var payload = line["data:".Length..].Trim();
            if (payload == "[DONE]")
            {
                break;
            }

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                if (!root.TryGetProperty("choices", out var choices) ||
                    choices.ValueKind != JsonValueKind.Array ||
                    choices.GetArrayLength() == 0)
                {
                    continue;
                }

                var choice = choices[0];
                string? token = null;

                if (choice.TryGetProperty("text", out var textProp) &&
                    textProp.ValueKind == JsonValueKind.String)
                {
                    token = textProp.GetString();
                }
                else if (choice.TryGetProperty("delta", out var delta) &&
                         delta.ValueKind == JsonValueKind.Object &&
                         delta.TryGetProperty("content", out var contentProp) &&
                         contentProp.ValueKind == JsonValueKind.String)
                {
                    token = contentProp.GetString();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    full.Append(token);
                    await onDelta(token, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse llama.cpp streaming chunk: {Chunk}", payload);
            }
        }

        return full.ToString();
    }
}
