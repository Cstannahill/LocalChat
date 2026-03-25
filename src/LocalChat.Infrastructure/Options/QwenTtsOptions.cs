namespace LocalChat.Infrastructure.Options;

public sealed class QwenTtsOptions
{
    public string BaseUrl { get; set; } = "http://localhost:3000/api/v1/";

    public string? ApiKey { get; set; }

    public string Model { get; set; } = "qwen3-tts-1.7b";

    public string DefaultVoice { get; set; } = "Camila";

    public double DefaultSpeed { get; set; } = 1.0;

    public string ResponseFormat { get; set; } = "wav";

    public int TimeoutSeconds { get; set; } = 120;
}
