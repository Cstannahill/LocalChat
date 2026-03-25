using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Features.Memory;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Options;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LocalChat.Infrastructure.BackgroundJobs;

public sealed class BackgroundMemoryProposalCoordinator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BackgroundMemoryProposalOptions _options;
    private readonly BackgroundMemoryProposalState _state;
    private readonly ILogger<BackgroundMemoryProposalCoordinator> _logger;
    private readonly SemaphoreSlim _sweepLock = new(1, 1);

    public BackgroundMemoryProposalCoordinator(
        IServiceScopeFactory scopeFactory,
        BackgroundMemoryProposalOptions options,
        BackgroundMemoryProposalState state,
        ILogger<BackgroundMemoryProposalCoordinator> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _state = state;
        _logger = logger;
    }

    public BackgroundProposalStatus GetStatus()
    {
        return new BackgroundProposalStatus
        {
            Enabled = _options.Enabled,
            IsSweepRunning = _state.IsSweepRunning,
            LastSweepStartedAt = _state.LastSweepStartedAt,
            LastSweepCompletedAt = _state.LastSweepCompletedAt,
            LastSweepMessage = _state.LastSweepMessage,
            LastSweepTriggeredConversationCount = _state.LastSweepTriggeredConversationCount,
            CooldownTrackedConversationCount = _state.CooldownTrackedConversationCount
        };
    }

    public async Task<BackgroundProposalRunResult> RunConversationNowAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Manual background proposal run requested for ConversationId={ConversationId}",
            conversationId);

        return await RunConversationInternalAsync(
            conversationId,
            ignoreCooldown: true,
            initiatedBy: "Manual admin trigger",
            cancellationToken);
    }

    public async Task RunScheduledSweepAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _state.LastSweepMessage = "Background memory proposals are disabled.";
            return;
        }

        if (!await _sweepLock.WaitAsync(0, cancellationToken))
        {
            _state.LastSweepMessage = "A background proposal sweep is already running.";
            return;
        }

        try
        {
            _state.IsSweepRunning = true;
            _state.LastSweepStartedAt = DateTime.UtcNow;
            _state.LastSweepTriggeredConversationCount = 0;

            _state.TrimOlderThan(TimeSpan.FromHours(12));

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var memoryRepository = scope.ServiceProvider.GetRequiredService<IMemoryRepository>();

            var expiredSceneStateCount = await memoryRepository.DeleteExpiredSceneStateAsync(
                DateTime.UtcNow,
                cancellationToken);

            var recentCutoff = DateTime.UtcNow.AddMinutes(-_options.RecentConversationWindowMinutes);

            var candidates = await dbContext.Conversations
                .AsNoTracking()
                .Where(x => x.UpdatedAt >= recentCutoff)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new ConversationSweepCandidate
                {
                    ConversationId = x.Id,
                    UpdatedAt = x.UpdatedAt,
                    MessageCount = dbContext.Messages.Count(m => m.ConversationId == x.Id)
                })
                .Take(_options.MaxConversationsPerSweep)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Background proposal sweep started. CandidateCount={CandidateCount}, RecentCutoff={RecentCutoff}, MaxConversationsPerSweep={MaxConversationsPerSweep}, ExpiredSceneStateDeleted={ExpiredSceneStateDeleted}",
                candidates.Count,
                recentCutoff,
                _options.MaxConversationsPerSweep,
                expiredSceneStateCount);

            var triggeredCount = 0;
            var skippedForMessageCount = 0;
            var skippedForCooldown = 0;
            var skippedForPending = 0;

            foreach (var candidate in candidates)
            {
                if (candidate.MessageCount < _options.MinConversationMessageCount)
                {
                    skippedForMessageCount++;
                    continue;
                }

                if (_state.TryGetLastRun(candidate.ConversationId, out var lastRunUtc))
                {
                    var minNextRun = lastRunUtc.AddMinutes(_options.MinMinutesBetweenRunsPerConversation);
                    if (DateTime.UtcNow < minNextRun)
                    {
                        skippedForCooldown++;
                        continue;
                    }
                }

                if (_options.SkipWhenPendingProposalsExist)
                {
                    var hasPending = await dbContext.MemoryItems
                        .AsNoTracking()
                        .AnyAsync(
                            x => x.ConversationId == candidate.ConversationId &&
                                 x.ReviewStatus == MemoryReviewStatus.Proposed,
                            cancellationToken);

                    if (hasPending)
                    {
                        skippedForPending++;
                        continue;
                    }
                }

                var result = await RunConversationInternalAsync(
                    candidate.ConversationId,
                    ignoreCooldown: false,
                    initiatedBy: "Scheduled sweep",
                    cancellationToken);

                if (result.Succeeded)
                {
                    triggeredCount++;
                }
            }

            _state.LastSweepTriggeredConversationCount = triggeredCount;
            _state.LastSweepMessage = $"Scheduled sweep completed. Triggered {triggeredCount} conversation(s).";
            _state.LastSweepCompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Background proposal sweep completed. Triggered={TriggeredCount}, SkippedForMessageCount={SkippedForMessageCount}, SkippedForCooldown={SkippedForCooldown}, SkippedForPending={SkippedForPending}",
                triggeredCount,
                skippedForMessageCount,
                skippedForCooldown,
                skippedForPending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background memory proposal sweep failed.");
            _state.LastSweepMessage = $"Sweep failed: {ex.Message}";
            _state.LastSweepCompletedAt = DateTime.UtcNow;
        }
        finally
        {
            _state.IsSweepRunning = false;
            _sweepLock.Release();
        }
    }

    private async Task<BackgroundProposalRunResult> RunConversationInternalAsync(
        Guid conversationId,
        bool ignoreCooldown,
        string initiatedBy,
        CancellationToken cancellationToken)
    {
        if (!ignoreCooldown &&
            _state.TryGetLastRun(conversationId, out var lastRunUtc))
        {
            var minNextRun = lastRunUtc.AddMinutes(_options.MinMinutesBetweenRunsPerConversation);
            if (DateTime.UtcNow < minNextRun)
            {
                return new BackgroundProposalRunResult
                {
                    Succeeded = false,
                    ConversationId = conversationId,
                    Message = $"Skipped due to cooldown. Last run at {lastRunUtc:u}.",
                    AttemptedCandidates = 0,
                    CreatedProposalCount = 0,
                    AutoSavedSceneStateCount = 0,
                    AutoAcceptedDurableCount = 0,
                    SkippedLowConfidenceCount = 0,
                    SkippedDuplicateCount = 0,
                    ConflictAnnotatedCount = 0,
                    InvalidCandidateCount = 0
                };
            }
        }

        using var scope = _scopeFactory.CreateScope();
        var proposalService = scope.ServiceProvider.GetRequiredService<IMemoryProposalService>();
        var retrievalSyncService = scope.ServiceProvider.GetRequiredService<IConversationRetrievalSyncService>();

        try
        {
            var result = await proposalService.GenerateForConversationAsync(conversationId, cancellationToken);

            if (result.AutoSavedSceneStateCount > 0 || result.AutoAcceptedDurableCount > 0)
            {
                await retrievalSyncService.ReindexConversationAsync(conversationId, cancellationToken);
            }

            _state.MarkRun(conversationId, DateTime.UtcNow);

            _logger.LogInformation(
                "Background memory proposal run succeeded. ConversationId={ConversationId}, InitiatedBy={InitiatedBy}, Attempted={AttemptedCandidates}, CreatedProposals={CreatedProposalCount}, AutoSavedSceneState={AutoSavedSceneStateCount}, AutoAcceptedDurable={AutoAcceptedDurableCount}, SkippedLowConfidence={SkippedLowConfidenceCount}, SkippedDuplicates={SkippedDuplicateCount}, ConflictsAnnotated={ConflictAnnotatedCount}, Invalid={InvalidCandidateCount}",
                conversationId,
                initiatedBy,
                result.AttemptedCandidates,
                result.CreatedProposalCount,
                result.AutoSavedSceneStateCount,
                result.AutoAcceptedDurableCount,
                result.SkippedLowConfidenceCount,
                result.SkippedDuplicateCount,
                result.ConflictAnnotatedCount,
                result.InvalidCandidateCount);

            return new BackgroundProposalRunResult
            {
                Succeeded = true,
                ConversationId = conversationId,
                Message = $"{initiatedBy} completed for conversation '{conversationId}'.",
                AttemptedCandidates = result.AttemptedCandidates,
                CreatedProposalCount = result.CreatedProposalCount,
                AutoSavedSceneStateCount = result.AutoSavedSceneStateCount,
                AutoAcceptedDurableCount = result.AutoAcceptedDurableCount,
                SkippedLowConfidenceCount = result.SkippedLowConfidenceCount,
                SkippedDuplicateCount = result.SkippedDuplicateCount,
                ConflictAnnotatedCount = result.ConflictAnnotatedCount,
                InvalidCandidateCount = result.InvalidCandidateCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Background memory proposal run failed. ConversationId={ConversationId}, InitiatedBy={InitiatedBy}",
                conversationId,
                initiatedBy);

            return new BackgroundProposalRunResult
            {
                Succeeded = false,
                ConversationId = conversationId,
                Message = $"Background proposal generation failed: {ex.Message}",
                AttemptedCandidates = 0,
                CreatedProposalCount = 0,
                AutoSavedSceneStateCount = 0,
                AutoAcceptedDurableCount = 0,
                SkippedLowConfidenceCount = 0,
                SkippedDuplicateCount = 0,
                ConflictAnnotatedCount = 0,
                InvalidCandidateCount = 0
            };
        }
    }

    private sealed class ConversationSweepCandidate
    {
        public required Guid ConversationId { get; init; }

        public required DateTime UpdatedAt { get; init; }

        public required int MessageCount { get; init; }
    }
}
