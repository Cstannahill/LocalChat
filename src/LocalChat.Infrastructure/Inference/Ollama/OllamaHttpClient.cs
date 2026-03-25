using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaHttpClient(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> StreamGenerateAsync(
        string prompt,
        Func<string, CancellationToken, Task> onDelta,
        CancellationToken cancellationToken = default
    )
    {
        var request = new OllamaChatRequest
        {
            Model = _options.Model,
            Prompt = prompt,
            Stream = true,
            KeepAlive = _options.KeepAlive,
        };

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "generate")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Ollama request failed with status {(int)response.StatusCode}: {errorBody}"
            );
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var fullResponse = new StringBuilder();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var chunk = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line, JsonOptions);
            if (chunk is null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(chunk.Response))
            {
                fullResponse.Append(chunk.Response);
                await onDelta(chunk.Response, cancellationToken);
            }

            if (chunk.Done)
            {
                break;
            }
        }

        return fullResponse.ToString();
    }

    public async Task<float[]> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        var request = new OllamaEmbeddingRequest { Model = _options.EmbeddingModel, Input = text };

        using var response = await _httpClient.PostAsJsonAsync("embed", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Ollama embed request failed with status {(int)response.StatusCode}: {errorBody}"
            );
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
            cancellationToken: cancellationToken
        );

        var vector = payload?.Embeddings?.FirstOrDefault();
        if (vector is null || vector.Count == 0)
        {
            throw new InvalidOperationException(
                "Ollama embed response did not contain an embedding vector."
            );
        }

        return vector.ToArray();
    }
}
