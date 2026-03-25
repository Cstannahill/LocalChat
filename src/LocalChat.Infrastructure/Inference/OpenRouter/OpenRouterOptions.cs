namespace LocalChat.Infrastructure.Inference.OpenRouter;

public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    public string? ApiKey { get; set; }

    public string? DefaultModel { get; set; }

    public string? HttpReferer { get; set; }

    public string? AppTitle { get; set; }

    public bool EnableStreaming { get; set; } = true;

    public int DefaultContextWindow { get; set; } = 32768;
}
