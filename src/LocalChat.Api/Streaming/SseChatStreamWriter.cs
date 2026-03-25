using System.Text.Json;
using LocalChat.Contracts.Chat.Streaming;

namespace LocalChat.Api.Streaming;

public sealed class SseChatStreamWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task WriteEventAsync(
        HttpResponse response,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        await response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    public Task WriteStartedAsync(
        HttpResponse response,
        ChatStreamStartedEvent payload,
        CancellationToken cancellationToken = default)
    {
        return WriteEventAsync(response, SseEventNames.Started, payload, cancellationToken);
    }

    public Task WriteDeltaAsync(
        HttpResponse response,
        string delta,
        CancellationToken cancellationToken = default)
    {
        var payload = new ChatTokenDeltaEvent
        {
            Type = SseEventNames.TokenDelta,
            Delta = delta
        };

        return WriteEventAsync(response, SseEventNames.TokenDelta, payload, cancellationToken);
    }

    public Task WriteCompletedAsync(
        HttpResponse response,
        ChatCompletedEvent payload,
        CancellationToken cancellationToken = default)
    {
        return WriteEventAsync(response, SseEventNames.Completed, payload, cancellationToken);
    }

    public Task WriteErrorAsync(
        HttpResponse response,
        string message,
        CancellationToken cancellationToken = default)
    {
        var payload = new ChatErrorEvent
        {
            Type = SseEventNames.Error,
            Message = message
        };

        return WriteEventAsync(response, SseEventNames.Error, payload, cancellationToken);
    }
}