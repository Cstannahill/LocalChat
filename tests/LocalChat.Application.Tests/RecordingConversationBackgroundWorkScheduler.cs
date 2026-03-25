using LocalChat.Application.Background;

namespace LocalChat.Application.Tests;

public sealed class RecordingConversationBackgroundWorkScheduler : IConversationBackgroundWorkScheduler
{
    public int CallCount { get; private set; }

    public Guid LastConversationId { get; private set; }

    public ConversationBackgroundWorkType LastWorkType { get; private set; }

    public string? LastReason { get; private set; }

    public Task ScheduleConversationChangeAsync(
        Guid conversationId,
        ConversationBackgroundWorkType workType,
        string reason,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastConversationId = conversationId;
        LastWorkType = workType;
        LastReason = reason;
        return Task.CompletedTask;
    }
}
