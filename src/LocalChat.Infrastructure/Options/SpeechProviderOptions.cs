namespace LocalChat.Infrastructure.Options;

public sealed class SpeechProviderOptions
{
    public const string SectionName = "Speech";

    public string Provider { get; set; } = "Qwen";
}
