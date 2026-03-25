using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.Inference.HuggingFace;

public sealed class HuggingFaceInferenceProvider
{
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceOptions _options;
    private readonly ILogger<HuggingFaceInferenceProvider> _logger;

    public HuggingFaceInferenceProvider(
        HttpClient httpClient,
        HuggingFaceOptions options,
        ILogger<HuggingFaceInferenceProvider> logger)
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
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Hugging Face API key is not configured.");
        }

        var model = !string.IsNullOrWhiteSpace(executionSettings?.ModelIdentifier)
            ? executionSettings.ModelIdentifier
            : _options.DefaultModel;

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException("No Hugging Face model was provided.");
        }

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/chat/completions";

        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["stream"] = _options.EnableStreaming,
            ["messages"] = new object[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
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
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        if (_options.EnableStreaming)
        {
            return await StreamResponseAsync(request, onDelta, cancellationToken);
        }

        return await ReadNonStreamingResponseAsync(request, onDelta, cancellationToken);
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
                $"Hugging Face request failed with status {(int)response.StatusCode}: {errorBody}");
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

                if (choice.TryGetProperty("delta", out var delta) &&
                    delta.ValueKind == JsonValueKind.Object &&
                    delta.TryGetProperty("content", out var contentProp) &&
                    contentProp.ValueKind == JsonValueKind.String)
                {
                    token = contentProp.GetString();
                }
                else if (choice.TryGetProperty("message", out var message) &&
                         message.ValueKind == JsonValueKind.Object &&
                         message.TryGetProperty("content", out var messageContent) &&
                         messageContent.ValueKind == JsonValueKind.String)
                {
                    token = messageContent.GetString();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    full.Append(token);
                    await onDelta(token, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Hugging Face streaming chunk: {Chunk}", payload);
            }
        }

        return full.ToString();
    }

    private async Task<string> ReadNonStreamingResponseAsync(
        HttpRequestMessage request,
        Func<string, CancellationToken, Task> onDelta,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Hugging Face request failed with status {(int)response.StatusCode}: {errorBody}");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            if (!string.IsNullOrEmpty(content))
            {
                await onDelta(content, cancellationToken);
            }

            return content;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Hugging Face response parsing failed. Response body: {body}",
                ex);
        }
    }
}
