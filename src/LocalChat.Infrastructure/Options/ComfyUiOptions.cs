namespace LocalChat.Infrastructure.Options;

public sealed class ComfyUiOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8188/";

    public string WorkflowTemplatePath { get; set; } = "ComfyUI/text2img-workflow.json";

    public int PollIntervalMs { get; set; } = 1000;

    public int TimeoutSeconds { get; set; } = 300;

    public string PositivePromptNodeId { get; set; } = "6";

    public string? NegativePromptNodeId { get; set; }

    public string LatentNodeId { get; set; } = "5";

    public string SamplerNodeId { get; set; } = "3";

    public string SaveImageNodeId { get; set; } = "9";

    public string FilenamePrefix { get; set; } = "LocalChat";
}
