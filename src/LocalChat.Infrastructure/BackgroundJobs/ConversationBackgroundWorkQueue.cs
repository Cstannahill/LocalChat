using System.Threading.Channels;
using LocalChat.Application.Background;
using InfraConversationBackgroundWorkOptions = LocalChat.Infrastructure.Options.ConversationBackgroundWorkOptions;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class ConversationBackgroundWorkQueue : IConversationBackgroundWorkScheduler
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, PendingConversationBackgroundWork> _pending = new();
    private readonly Channel<bool> _signalChannel = Channel.CreateUnbounded<bool>();
    private readonly InfraConversationBackgroundWorkOptions _options;
    private readonly ILogger<ConversationBackgroundWorkQueue> _logger;

    public ConversationBackgroundWorkQueue(
        InfraConversationBackgroundWorkOptions options,
        ILogger<ConversationBackgroundWorkQueue> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task ScheduleConversationChangeAsync(
        Guid conversationId,
        ConversationBackgroundWorkType workType,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || workType == ConversationBackgroundWorkType.None)
        {
            return Task.CompletedTask;
        }

        var now = DateTime.UtcNow;

        lock (_gate)
        {
            if (!_pending.TryGetValue(conversationId, out var item))
            {
                item = new PendingConversationBackgroundWork
                {
                    ConversationId = conversationId
                };

                _pending[conversationId] = item;
            }

            item.LastReason = reason;
            item.LastScheduledAt = now;

            if (workType.HasFlag(ConversationBackgroundWorkType.RetrievalReindex))
            {
                item.RetrievalDueAt = now.AddMilliseconds(_options.RetrievalDebounceMilliseconds);
            }

            if (workType.HasFlag(ConversationBackgroundWorkType.MemoryExtraction))
            {
                item.MemoryDueAt = now.AddMilliseconds(_options.MemoryDebounceMilliseconds);
            }

            if (workType.HasFlag(ConversationBackgroundWorkType.SummaryRefresh))
            {
                item.SummaryDueAt = now.AddMilliseconds(_options.SummaryDebounceMilliseconds);
            }
        }

        _signalChannel.Writer.TryWrite(true);

        _logger.LogInformation(
            "Conversation background work scheduled. ConversationId={ConversationId}, WorkType={WorkType}, Reason={Reason}",
            conversationId,
            workType,
            reason);

        return Task.CompletedTask;
    }

    public async Task WaitForSignalOrTimeoutAsync(CancellationToken cancellationToken)
    {
        var waitToReadTask = _signalChannel.Reader.WaitToReadAsync(cancellationToken).AsTask();
        var delayTask = Task.Delay(_options.PollIntervalMilliseconds, cancellationToken);

        var completed = await Task.WhenAny(waitToReadTask, delayTask);

        if (completed == waitToReadTask && await waitToReadTask)
        {
            while (_signalChannel.Reader.TryRead(out _))
            {
            }
        }
    }

    public IReadOnlyList<ConversationBackgroundWorkExecution> TakeDueWork(DateTime utcNow)
    {
        var results = new List<ConversationBackgroundWorkExecution>();

        lock (_gate)
        {
            var keys = _pending.Keys.ToList();

            foreach (var key in keys)
            {
                var item = _pending[key];

                var runRetrieval = item.RetrievalDueAt is not null && item.RetrievalDueAt <= utcNow;
                var runMemory = item.MemoryDueAt is not null && item.MemoryDueAt <= utcNow;
                var runSummary = item.SummaryDueAt is not null && item.SummaryDueAt <= utcNow;

                if (!runRetrieval && !runMemory && !runSummary)
                {
                    continue;
                }

                if (runRetrieval)
                {
                    item.RetrievalDueAt = null;
                }

                if (runMemory)
                {
                    item.MemoryDueAt = null;
                }

                if (runSummary)
                {
                    item.SummaryDueAt = null;
                }

                if (item.RetrievalDueAt is null &&
                    item.MemoryDueAt is null &&
                    item.SummaryDueAt is null)
                {
                    _pending.Remove(key);
                }

                results.Add(new ConversationBackgroundWorkExecution
                {
                    ConversationId = item.ConversationId,
                    RunRetrievalReindex = runRetrieval,
                    RunMemoryExtraction = runMemory,
                    RunSummaryRefresh = runSummary,
                    Reason = item.LastReason
                });
            }
        }

        return results;
    }

    public ConversationBackgroundWorkQueueSnapshot GetSnapshot(DateTime utcNow)
    {
        lock (_gate)
        {
            var items = _pending.Values
                .OrderBy(x =>
                    new[]
                    {
                        x.RetrievalDueAt,
                        x.MemoryDueAt,
                        x.SummaryDueAt
                    }
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .DefaultIfEmpty(DateTime.MaxValue)
                    .Min())
                .Select(x => new ConversationBackgroundWorkQueueItemSnapshot
                {
                    ConversationId = x.ConversationId,
                    LastReason = x.LastReason,
                    LastScheduledAt = x.LastScheduledAt,
                    RetrievalDueAt = x.RetrievalDueAt,
                    MemoryDueAt = x.MemoryDueAt,
                    SummaryDueAt = x.SummaryDueAt,
                    RetrievalDueNow = x.RetrievalDueAt is not null && x.RetrievalDueAt <= utcNow,
                    MemoryDueNow = x.MemoryDueAt is not null && x.MemoryDueAt <= utcNow,
                    SummaryDueNow = x.SummaryDueAt is not null && x.SummaryDueAt <= utcNow
                })
                .ToList();

            return new ConversationBackgroundWorkQueueSnapshot
            {
                PendingConversationCount = items.Count,
                PendingItems = items
            };
        }
    }

    private sealed class PendingConversationBackgroundWork
    {
        public required Guid ConversationId { get; init; }

        public string? LastReason { get; set; }

        public DateTime LastScheduledAt { get; set; }

        public DateTime? RetrievalDueAt { get; set; }

        public DateTime? MemoryDueAt { get; set; }

        public DateTime? SummaryDueAt { get; set; }
    }
}

public sealed class ConversationBackgroundWorkExecution
{
    public required Guid ConversationId { get; init; }

    public bool RunRetrievalReindex { get; init; }

    public bool RunMemoryExtraction { get; init; }

    public bool RunSummaryRefresh { get; init; }

    public string? Reason { get; init; }
}

public sealed class ConversationBackgroundWorkQueueSnapshot
{
    public required int PendingConversationCount { get; init; }

    public required IReadOnlyList<ConversationBackgroundWorkQueueItemSnapshot> PendingItems { get; init; }
}

public sealed class ConversationBackgroundWorkQueueItemSnapshot
{
    public required Guid ConversationId { get; init; }

    public string? LastReason { get; init; }

    public DateTime LastScheduledAt { get; init; }

    public DateTime? RetrievalDueAt { get; init; }

    public DateTime? MemoryDueAt { get; init; }

    public DateTime? SummaryDueAt { get; init; }

    public bool RetrievalDueNow { get; init; }

    public bool MemoryDueNow { get; init; }

    public bool SummaryDueNow { get; init; }
}
