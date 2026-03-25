using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Features.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class ConversationBackgroundWorkExecutor
{
    private readonly IMemoryProposalService _proposalService;
    private readonly IConversationRetrievalSyncService _retrievalSyncService;
    private readonly IConversationSummaryRefreshService _summaryRefreshService;
    private readonly ILogger<ConversationBackgroundWorkExecutor> _logger;

    public ConversationBackgroundWorkExecutor(
        IMemoryProposalService proposalService,
        IConversationRetrievalSyncService retrievalSyncService,
        IConversationSummaryRefreshService summaryRefreshService)
        : this(
            proposalService,
            retrievalSyncService,
            summaryRefreshService,
            NullLogger<ConversationBackgroundWorkExecutor>.Instance)
    {
    }

    public ConversationBackgroundWorkExecutor(
        IMemoryProposalService proposalService,
        IConversationRetrievalSyncService retrievalSyncService,
        IConversationSummaryRefreshService summaryRefreshService,
        ILogger<ConversationBackgroundWorkExecutor> logger)
    {
        _proposalService = proposalService;
        _retrievalSyncService = retrievalSyncService;
        _summaryRefreshService = summaryRefreshService;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        ConversationBackgroundWorkExecution work,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing conversation background work. ConversationId={ConversationId}, Retrieval={RunRetrieval}, Memory={RunMemory}, Summary={RunSummary}, Reason={Reason}",
            work.ConversationId,
            work.RunRetrievalReindex,
            work.RunMemoryExtraction,
            work.RunSummaryRefresh,
            work.Reason);

        var shouldReindexAfterMemory = false;

        if (work.RunMemoryExtraction)
        {
            try
            {
                var proposalResult = await _proposalService.GenerateForConversationAsync(
                    work.ConversationId,
                    cancellationToken);

                shouldReindexAfterMemory =
                    proposalResult.AutoSavedSceneStateCount > 0 ||
                    proposalResult.AutoAcceptedDurableCount > 0;

                _logger.LogInformation(
                    "Conversation memory extraction completed. ConversationId={ConversationId}, Attempted={Attempted}, Proposals={Proposals}, AutoSceneState={AutoSceneState}, AutoAcceptedDurable={AutoAcceptedDurable}",
                    work.ConversationId,
                    proposalResult.AttemptedCandidates,
                    proposalResult.CreatedProposalCount,
                    proposalResult.AutoSavedSceneStateCount,
                    proposalResult.AutoAcceptedDurableCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Conversation memory extraction failed. ConversationId={ConversationId}",
                    work.ConversationId);
            }
        }

        if (work.RunRetrievalReindex || shouldReindexAfterMemory)
        {
            try
            {
                await _retrievalSyncService.ReindexConversationAsync(
                    work.ConversationId,
                    cancellationToken);

                _logger.LogInformation(
                    "Conversation retrieval reindex completed. ConversationId={ConversationId}, TriggeredByMemory={TriggeredByMemory}",
                    work.ConversationId,
                    shouldReindexAfterMemory);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Conversation retrieval reindex failed. ConversationId={ConversationId}",
                    work.ConversationId);
            }
        }

        if (work.RunSummaryRefresh)
        {
            try
            {
                var summaryResult = await _summaryRefreshService.RefreshAsync(
                    work.ConversationId,
                    cancellationToken);

                _logger.LogInformation(
                    "Conversation summary refresh processed. ConversationId={ConversationId}, Refreshed={Refreshed}, Reason={Reason}, EndSequence={EndSequence}",
                    work.ConversationId,
                    summaryResult.Refreshed,
                    summaryResult.Reason,
                    summaryResult.EndSequenceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Conversation summary refresh failed. ConversationId={ConversationId}",
                    work.ConversationId);
            }
        }
    }
}
