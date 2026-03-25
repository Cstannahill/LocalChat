using System.Diagnostics;
using LocalChat.Api.Telemetry;
using Microsoft.Extensions.Options;

namespace LocalChat.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestFlowLogWriter _logWriter;
    private readonly RequestFlowLoggingOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        RequestFlowLogWriter logWriter,
        IOptions<RequestFlowLoggingOptions> options,
        IWebHostEnvironment environment,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logWriter = logWriter;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldTrackRequest(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var startedAtUtc = DateTime.UtcNow;
        var state = new RequestFlowState(
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty,
            startedAtUtc);
        context.Items[RequestFlowState.HttpContextItemKey] = state;

        var stopwatch = Stopwatch.StartNew();
        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var summary = state.BuildSummary(
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                capturedException?.GetType().Name);

            _logger.LogInformation(
                "Request flow completed. RequestId={RequestId}, {Method} {Path}, StatusCode={StatusCode}, TotalMs={TotalMs}, StageCount={StageCount}",
                summary.RequestId,
                summary.Method,
                summary.Path,
                summary.StatusCode,
                summary.TotalElapsedMs,
                summary.Stages.Count);

            if (_options.Enabled)
            {
                var logFilePath = ResolveLogFilePath(_options.LogFilePath);

                try
                {
                    await _logWriter.WriteAsync(logFilePath, summary, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to persist request flow log. RequestId={RequestId}, Path={LogPath}",
                        summary.RequestId,
                        logFilePath);
                }
            }
        }
    }

    private static bool ShouldTrackRequest(PathString path)
    {
        if (!path.HasValue)
        {
            return false;
        }

        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveLogFilePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configuredPath));
    }
}
