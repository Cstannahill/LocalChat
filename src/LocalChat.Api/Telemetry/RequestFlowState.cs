namespace LocalChat.Api.Telemetry;

internal sealed class RequestFlowState
{
    internal const string HttpContextItemKey = "__localchat_request_flow_state";

    private readonly object _sync = new();
    private readonly List<RequestFlowStage> _stages = [];
    private readonly Dictionary<string, string> _tags = new(StringComparer.OrdinalIgnoreCase);

    public RequestFlowState(string requestId, string method, string path, DateTime startedAtUtc)
    {
        RequestId = requestId;
        Method = method;
        Path = path;
        StartedAtUtc = startedAtUtc;
    }

    public string RequestId { get; }

    public string Method { get; }

    public string Path { get; }

    public DateTime StartedAtUtc { get; }

    public void AddStage(RequestFlowStage stage)
    {
        lock (_sync)
        {
            _stages.Add(stage);
        }
    }

    public void AddTag(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        lock (_sync)
        {
            _tags[key] = value;
        }
    }

    public RequestFlowSummary BuildSummary(int statusCode, long totalElapsedMs, string? error)
    {
        lock (_sync)
        {
            return new RequestFlowSummary
            {
                TimestampUtc = DateTime.UtcNow,
                RequestId = RequestId,
                Method = Method,
                Path = Path,
                StatusCode = statusCode,
                TotalElapsedMs = totalElapsedMs,
                Error = error,
                Tags = _tags.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
                Stages = _stages.OrderBy(x => x.StartedAtUtc).ToList()
            };
        }
    }
}

internal sealed class RequestFlowSummary
{
    public DateTime TimestampUtc { get; init; }

    public required string RequestId { get; init; }

    public required string Method { get; init; }

    public required string Path { get; init; }

    public int StatusCode { get; init; }

    public long TotalElapsedMs { get; init; }

    public string? Error { get; init; }

    public required IReadOnlyDictionary<string, string> Tags { get; init; }

    public required IReadOnlyList<RequestFlowStage> Stages { get; init; }
}

internal sealed class RequestFlowStage
{
    public required string Name { get; init; }

    public DateTime StartedAtUtc { get; init; }

    public long ElapsedMs { get; init; }
}

