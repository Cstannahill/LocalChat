namespace LocalChat.Application.Abstractions.Telemetry;

public interface IRequestFlowTiming
{
    IDisposable BeginStage(string stageName);

    void AddTag(string key, string? value);
}

