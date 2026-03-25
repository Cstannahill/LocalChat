using System.Text.Json;

namespace LocalChat.Api.Telemetry;

public sealed class RequestFlowLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SemaphoreSlim _writeLock = new(1, 1);

    internal async Task WriteAsync(
        string absolutePath,
        RequestFlowSummary summary,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var line = JsonSerializer.Serialize(summary, JsonOptions);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(
                absolutePath,
                line + Environment.NewLine,
                cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
