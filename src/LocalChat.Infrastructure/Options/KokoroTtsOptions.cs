namespace LocalChat.Infrastructure.Options;

public sealed class KokoroTtsOptions
{
    public string BaseUrl { get; set; } = "http://localhost:3000/api/v1/";

    public string? ApiKey { get; set; }

    public string Model { get; set; } = "model_q8f16";

    public string DefaultVoice { get; set; } = "af_heart";

    public double DefaultSpeed { get; set; } = 1.0;

    public string ResponseFormat { get; set; } = "mp3";

    public string PublicBasePath { get; set; } = "/generated/audio";

    public int TimeoutSeconds { get; set; } = 120;
}
