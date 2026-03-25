namespace LocalChat.Application.Abstractions.Telemetry;

public sealed class NullRequestFlowTiming : IRequestFlowTiming
{
    public static readonly NullRequestFlowTiming Instance = new();

    private static readonly IDisposable NoOpScope = new NoOpDisposable();

    private NullRequestFlowTiming()
    {
    }

    public IDisposable BeginStage(string stageName)
    {
        return NoOpScope;
    }

    public void AddTag(string key, string? value)
    {
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

