using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class ConversationBackgroundWorkHostedService : BackgroundService
{
    private readonly ConversationBackgroundWorkQueue _queue;
    private readonly ConversationBackgroundWorkProcessor _processor;
    private readonly ILogger<ConversationBackgroundWorkHostedService> _logger;

    public ConversationBackgroundWorkHostedService(
        ConversationBackgroundWorkQueue queue,
        ConversationBackgroundWorkProcessor processor,
        ILogger<ConversationBackgroundWorkHostedService> logger)
    {
        _queue = queue;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Conversation background work hosted service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _queue.WaitForSignalOrTimeoutAsync(stoppingToken);
                await _processor.ProcessDueWorkAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Conversation background work loop failed.");
            }
        }

        _logger.LogInformation("Conversation background work hosted service stopped.");
    }
}
