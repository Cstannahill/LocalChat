using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Features.Memory;
using LocalChat.Infrastructure.BackgroundJobs;

namespace LocalChat.Infrastructure.Tests;

public sealed class ConversationBackgroundWorkExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_ReindexesAfterMemoryExtraction_WhenSessionStateWasAutoSaved()
    {
        var proposalService = new FakeMemoryProposalService
        {
            NextResult = new MemoryProposalGenerationResult
            {
                AttemptedCandidates = 2,
                CreatedProposalCount = 0,
                AutoSavedSessionStateCount = 1,
                AutoAcceptedDurableCount = 0,
                SessionStateReplacedCount = 0,
                MergedDurableProposalCount = 0,
                ConflictingDurableProposalCount = 0,
                SkippedLowConfidenceCount = 0,
                SkippedDuplicateCount = 0,
                ConflictAnnotatedCount = 0,
                InvalidCandidateCount = 0
            }
        };

        var retrievalSync = new FakeConversationRetrievalSyncService();
        var summaryService = new FakeConversationSummaryRefreshService();

        var executor = new ConversationBackgroundWorkExecutor(
            proposalService,
            retrievalSync,
            summaryService);

        var conversationId = Guid.NewGuid();

        await executor.ExecuteAsync(new ConversationBackgroundWorkExecution
        {
            ConversationId = conversationId,
            RunRetrievalReindex = false,
            RunMemoryExtraction = true,
            RunSummaryRefresh = false,
            Reason = "test"
        });

        Assert.Equal(1, proposalService.CallCount);
        Assert.Equal(1, retrievalSync.CallCount);
        Assert.Equal(0, summaryService.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_RunsSummaryRefresh_WhenRequested()
    {
        var proposalService = new FakeMemoryProposalService();
        var retrievalSync = new FakeConversationRetrievalSyncService();
        var summaryService = new FakeConversationSummaryRefreshService();

        var executor = new ConversationBackgroundWorkExecutor(
            proposalService,
            retrievalSync,
            summaryService);

        var conversationId = Guid.NewGuid();

        await executor.ExecuteAsync(new ConversationBackgroundWorkExecution
        {
            ConversationId = conversationId,
            RunRetrievalReindex = false,
            RunMemoryExtraction = false,
            RunSummaryRefresh = true,
            Reason = "summary-only"
        });

        Assert.Equal(0, proposalService.CallCount);
        Assert.Equal(0, retrievalSync.CallCount);
        Assert.Equal(1, summaryService.CallCount);
    }

    private sealed class FakeMemoryProposalService : IMemoryProposalService
    {
        public int CallCount { get; private set; }

        public MemoryProposalGenerationResult NextResult { get; set; } =
            new()
            {
                AttemptedCandidates = 0,
                CreatedProposalCount = 0,
                AutoSavedSessionStateCount = 0,
                AutoAcceptedDurableCount = 0,
                SessionStateReplacedCount = 0,
                MergedDurableProposalCount = 0,
                ConflictingDurableProposalCount = 0,
                SkippedLowConfidenceCount = 0,
                SkippedDuplicateCount = 0,
                ConflictAnnotatedCount = 0,
                InvalidCandidateCount = 0
            };

        public Task<MemoryProposalGenerationResult> GenerateForConversationAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(NextResult);
        }
    }

    private sealed class FakeConversationRetrievalSyncService : IConversationRetrievalSyncService
    {
        public int CallCount { get; private set; }

        public Task ReindexConversationAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeConversationSummaryRefreshService : IConversationSummaryRefreshService
    {
        public int CallCount { get; private set; }

        public Task<ConversationSummaryRefreshResult> RefreshAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            return Task.FromResult(new ConversationSummaryRefreshResult
            {
                ConversationId = conversationId,
                Refreshed = true,
                Reason = "refreshed",
                StartSequenceNumber = 1,
                EndSequenceNumber = 10,
                SummaryText = "summary"
            });
        }
    }
}
