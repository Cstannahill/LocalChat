using System.Net.Http.Json;
using LocalChat.Application.Abstractions.Speech;
using LocalChat.Infrastructure.Options;

namespace LocalChat.Infrastructure.Speech.Qwen;

public sealed class QwenSpeechSynthesisProvider : ISpeechSynthesisProvider
{
    private readonly HttpClient _httpClient;
    private readonly QwenTtsOptions _options;

    public QwenSpeechSynthesisProvider(
        HttpClient httpClient,
        QwenTtsOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<SpeechSynthesisResult> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken = default)
    {
        var effectiveVoice = !string.IsNullOrWhiteSpace(request.Voice)
            ? request.Voice.Trim()
            : _options.DefaultVoice;

        var effectiveModelIdentifier = !string.IsNullOrWhiteSpace(request.ModelIdentifier)
            ? request.ModelIdentifier.Trim()
            : _options.Model;

        var effectiveResponseFormat = !string.IsNullOrWhiteSpace(request.ResponseFormat)
            ? request.ResponseFormat.Trim()
            : _options.ResponseFormat;

        var effectiveSpeed = request.Speed ?? _options.DefaultSpeed;

        var dto = new QwenSpeechRequestDto
        {
            Model = effectiveModelIdentifier,
            Voice = effectiveVoice,
            Input = request.Input,
            ResponseFormat = effectiveResponseFormat,
            Speed = effectiveSpeed
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "audio/speech",
            dto,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var contentType = response.Content.Headers.ContentType?.MediaType switch
        {
            string mediaType when !string.IsNullOrWhiteSpace(mediaType) => mediaType,
            _ when string.Equals(effectiveResponseFormat, "mp3", StringComparison.OrdinalIgnoreCase) => "audio/mpeg",
            _ when string.Equals(effectiveResponseFormat, "wav", StringComparison.OrdinalIgnoreCase) => "audio/wav",
            _ => "application/octet-stream"
        };

        return new SpeechSynthesisResult
        {
            AudioBytes = bytes,
            ContentType = contentType,
            ResponseFormat = effectiveResponseFormat,
            EffectiveVoice = effectiveVoice,
            EffectiveModelIdentifier = effectiveModelIdentifier,
            EffectiveSpeed = effectiveSpeed
        };
    }
}
