namespace LocalChat.Application.Options;

public sealed class InspectionOptions
{
    public const string SectionName = "Inspection";

    public bool EnableRetrievalTelemetry { get; set; } = false;

    public bool EnablePromptTelemetry { get; set; } = false;

    public bool EnableFlowTimingTelemetry { get; set; } = false;
}
