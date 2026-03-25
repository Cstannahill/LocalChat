using System.Diagnostics;
using LocalChat.Application.Abstractions.Telemetry;

namespace LocalChat.Api.Telemetry;

public sealed class RequestFlowTiming : IRequestFlowTiming
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestFlowTiming(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IDisposable BeginStage(string stageName)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return NullRequestFlowTiming.Instance.BeginStage(stageName);
        }

        if (!context.Items.TryGetValue(RequestFlowState.HttpContextItemKey, out var boxedState)
            || boxedState is not RequestFlowState state)
        {
            return NullRequestFlowTiming.Instance.BeginStage(stageName);
        }

        var startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        return new StageScope(
            stageName,
            startedAtUtc,
            stopwatch,
            completed =>
            {
                state.AddStage(new RequestFlowStage
                {
                    Name = completed.Name,
                    StartedAtUtc = completed.StartedAtUtc,
                    ElapsedMs = completed.ElapsedMs
                });
            });
    }

    public void AddTag(string key, string? value)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        if (!context.Items.TryGetValue(RequestFlowState.HttpContextItemKey, out var boxedState)
            || boxedState is not RequestFlowState state)
        {
            return;
        }

        state.AddTag(key, value);
    }

    private sealed class StageScope : IDisposable
    {
        private readonly string _name;
        private readonly DateTime _startedAtUtc;
        private readonly Stopwatch _stopwatch;
        private readonly Action<CompletedStageScope> _onDispose;
        private bool _disposed;

        public StageScope(
            string name,
            DateTime startedAtUtc,
            Stopwatch stopwatch,
            Action<CompletedStageScope> onDispose)
        {
            _name = name;
            _startedAtUtc = startedAtUtc;
            _stopwatch = stopwatch;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _stopwatch.Stop();

            _onDispose(new CompletedStageScope
            {
                Name = _name,
                StartedAtUtc = _startedAtUtc,
                ElapsedMs = _stopwatch.ElapsedMilliseconds
            });
        }
    }

    private sealed class CompletedStageScope
    {
        public required string Name { get; init; }

        public DateTime StartedAtUtc { get; init; }

        public long ElapsedMs { get; init; }
    }
}

