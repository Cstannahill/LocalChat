using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaModelInfoClient
{
    private readonly HttpClient _httpClient;

    public OllamaModelInfoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int?> GetContextWindowAsync(
        string modelIdentifier,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "show",
            new OllamaShowRequest { Model = modelIdentifier },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<OllamaShowResponse>(cancellationToken: cancellationToken);
        if (dto is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Parameters))
        {
            var lines = dto.Parameters.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("num_ctx", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (parts.Length >= 2 && int.TryParse(parts[^1], out var parsed))
                    {
                        return parsed;
                    }
                }
            }
        }

        if (dto.ModelInfo is not null)
        {
            foreach (var key in new[]
                     {
                         "llama.context_length",
                         "qwen2.context_length",
                         "qwen3.context_length",
                         "gemma.context_length"
                     })
            {
                if (dto.ModelInfo.TryGetValue(key, out var value) &&
                    value is not null &&
                    int.TryParse(value.ToString(), out var parsed))
                {
                    return parsed;
                }
            }
        }

        return null;
    }
}

public sealed class OllamaShowRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }
}

public sealed class OllamaShowResponse
{
    [JsonPropertyName("parameters")]
    public string? Parameters { get; init; }

    [JsonPropertyName("model_info")]
    public Dictionary<string, object>? ModelInfo { get; init; }
}
