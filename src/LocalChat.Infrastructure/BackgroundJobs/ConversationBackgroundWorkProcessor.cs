using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class ConversationBackgroundWorkProcessor
{
    private readonly ConversationBackgroundWorkQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConversationBackgroundWorkProcessor> _logger;

    public ConversationBackgroundWorkProcessor(
        ConversationBackgroundWorkQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ConversationBackgroundWorkProcessor> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<int> ProcessDueWorkAsync(CancellationToken cancellationToken = default)
    {
        var dueWork = _queue.TakeDueWork(DateTime.UtcNow);
        if (dueWork.Count == 0)
        {
            return 0;
        }

        foreach (var work in dueWork)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var executor = scope.ServiceProvider.GetRequiredService<ConversationBackgroundWorkExecutor>();

                await executor.ExecuteAsync(work, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Conversation background work execution failed. ConversationId={ConversationId}",
                    work.ConversationId);
            }
        }

        return dueWork.Count;
    }
}
