using System.Collections.Concurrent;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class BackgroundMemoryProposalState
{
    private readonly ConcurrentDictionary<Guid, DateTime> _lastRunByConversation = new();

    public bool IsSweepRunning { get; set; }

    public DateTime? LastSweepStartedAt { get; set; }

    public DateTime? LastSweepCompletedAt { get; set; }

    public string? LastSweepMessage { get; set; }

    public int LastSweepTriggeredConversationCount { get; set; }

    public int CooldownTrackedConversationCount => _lastRunByConversation.Count;

    public bool TryGetLastRun(Guid conversationId, out DateTime lastRunUtc)
    {
        return _lastRunByConversation.TryGetValue(conversationId, out lastRunUtc);
    }

    public void MarkRun(Guid conversationId, DateTime runUtc)
    {
        _lastRunByConversation[conversationId] = runUtc;
    }

    public void TrimOlderThan(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow.Subtract(maxAge);

        foreach (var pair in _lastRunByConversation)
        {
            if (pair.Value < cutoff)
            {
                _lastRunByConversation.TryRemove(pair.Key, out _);
            }
        }
    }
}
