namespace LocalChat.Infrastructure.Inference.Ollama;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434/api/";

    public string Model { get; set; } = "Qwen35-2B-GPT";

    public string EmbeddingModel { get; set; } = "embeddinggemma";

    public int TimeoutSeconds { get; set; } = 120;

    public string KeepAlive { get; set; } = "30s";

    public int ReservedOutputTokens { get; set; } = 4096;

    public int SafetyMarginTokens { get; set; } = 1024;

    public int MaxContextFallback { get; set; } = 8192;
}
