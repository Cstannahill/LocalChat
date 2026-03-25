using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace LocalChat.Infrastructure.ImageGeneration.ComfyUi;

public sealed class ComfyUiQueuePromptRequest
{
    [JsonPropertyName("prompt")]
    public required JsonObject Prompt { get; init; }

    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("prompt_id")]
    public required string PromptId { get; init; }
}
