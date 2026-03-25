namespace LocalChat.Api.Telemetry;

public sealed class RequestFlowLoggingOptions
{
    public const string SectionName = "RequestFlowLogging";

    public bool Enabled { get; set; } = true;

    public string LogFilePath { get; set; } = "App_Data/Logs/request-flow.ndjson";
}

