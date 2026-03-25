using LocalChat.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class BackgroundMemoryProposalWorker : BackgroundService
{
    private readonly BackgroundMemoryProposalCoordinator _coordinator;
    private readonly BackgroundMemoryProposalOptions _options;
    private readonly ILogger<BackgroundMemoryProposalWorker> _logger;

    public BackgroundMemoryProposalWorker(
        BackgroundMemoryProposalCoordinator coordinator,
        BackgroundMemoryProposalOptions options,
        ILogger<BackgroundMemoryProposalWorker> logger)
    {
        _coordinator = coordinator;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background memory proposal worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _coordinator.RunScheduledSweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in background memory proposal worker.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Max(10, _options.ScanIntervalSeconds)),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Background memory proposal worker stopped.");
    }
}
