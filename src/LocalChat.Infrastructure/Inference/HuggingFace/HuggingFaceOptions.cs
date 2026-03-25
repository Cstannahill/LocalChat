namespace LocalChat.Infrastructure.Inference.HuggingFace;

public sealed class HuggingFaceOptions
{
    public const string SectionName = "HuggingFace";

    public string BaseUrl { get; set; } = "https://router.huggingface.co/v1";

    public string? ApiKey { get; set; }

    public string? DefaultModel { get; set; }

    public bool EnableStreaming { get; set; } = true;

    public int DefaultContextWindow { get; set; } = 32768;
}
