namespace LocalChat.Infrastructure.Inference.LlamaCpp;

public sealed class LlamaCppOptions
{
    public const string SectionName = "LlamaCpp";

    public string BaseUrl { get; set; } = "http://localhost:8080";

    public string? ApiKey { get; set; }

    public string? DefaultModel { get; set; }

    public int DefaultContextWindow { get; set; } = 32768;

    public bool UsePropsForContext { get; set; } = true;

    public int ReservedOutputTokens { get; set; } = 4096;

    public int SafetyMarginTokens { get; set; } = 1024;
}
