using LocalChat.Application.Background;
using LocalChat.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalChat.Infrastructure.Tests;

public sealed class ConversationBackgroundWorkQueueTests
{
    [Fact]
    public async Task ScheduleConversationChangeAsync_CoalescesWorkForSameConversation_AndRespectsDebounceWindows()
    {
        var options = new LocalChat.Infrastructure.Options.ConversationBackgroundWorkOptions
        {
            RetrievalDebounceMilliseconds = 100,
            MemoryDebounceMilliseconds = 300,
            SummaryDebounceMilliseconds = 500
        };

        var queue = new ConversationBackgroundWorkQueue(
            options,
            NullLogger<ConversationBackgroundWorkQueue>.Instance);

        var conversationId = Guid.NewGuid();

        await queue.ScheduleConversationChangeAsync(
            conversationId,
            ConversationBackgroundWorkType.RetrievalReindex,
            "initial-retrieval");

        await queue.ScheduleConversationChangeAsync(
            conversationId,
            ConversationBackgroundWorkType.MemoryExtraction,
            "memory-followup");

        var early = queue.TakeDueWork(DateTime.UtcNow.AddMilliseconds(150));

        Assert.Single(early);
        Assert.Equal(conversationId, early[0].ConversationId);
        Assert.True(early[0].RunRetrievalReindex);
        Assert.False(early[0].RunMemoryExtraction);
        Assert.False(early[0].RunSummaryRefresh);

        var later = queue.TakeDueWork(DateTime.UtcNow.AddMilliseconds(350));

        Assert.Single(later);
        Assert.Equal(conversationId, later[0].ConversationId);
        Assert.False(later[0].RunRetrievalReindex);
        Assert.True(later[0].RunMemoryExtraction);
        Assert.False(later[0].RunSummaryRefresh);
    }

    [Fact]
    public async Task ScheduleConversationChangeAsync_MergesRepeatedSchedulingIntoSingleExecution()
    {
        var options = new LocalChat.Infrastructure.Options.ConversationBackgroundWorkOptions
        {
            RetrievalDebounceMilliseconds = 50,
            MemoryDebounceMilliseconds = 50,
            SummaryDebounceMilliseconds = 50
        };

        var queue = new ConversationBackgroundWorkQueue(
            options,
            NullLogger<ConversationBackgroundWorkQueue>.Instance);

        var conversationId = Guid.NewGuid();

        await queue.ScheduleConversationChangeAsync(
            conversationId,
            ConversationBackgroundWorkType.RetrievalReindex,
            "first");

        await queue.ScheduleConversationChangeAsync(
            conversationId,
            ConversationBackgroundWorkType.RetrievalReindex | ConversationBackgroundWorkType.SummaryRefresh,
            "second");

        var due = queue.TakeDueWork(DateTime.UtcNow.AddMilliseconds(100));

        Assert.Single(due);
        Assert.Equal(conversationId, due[0].ConversationId);
        Assert.True(due[0].RunRetrievalReindex);
        Assert.False(due[0].RunMemoryExtraction);
        Assert.True(due[0].RunSummaryRefresh);
    }
}

