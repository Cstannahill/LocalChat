using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Features.Memory;
using LocalChat.Contracts.Admin;
using LocalChat.Infrastructure.BackgroundJobs;
using InfraConversationBackgroundWorkOptions = LocalChat.Infrastructure.Options.ConversationBackgroundWorkOptions;

namespace LocalChat.Api.Endpoints;

public static class BackgroundWorkEndpoints
{
    public static IEndpointRouteBuilder MapBackgroundWorkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/background-work")
            .WithTags("Admin");

        group.MapGet("/status", (
            ConversationBackgroundWorkQueue queue,
            InfraConversationBackgroundWorkOptions queueOptions,
            BackgroundMemoryProposalCoordinator proposalCoordinator) =>
        {
            var queueSnapshot = queue.GetSnapshot(DateTime.UtcNow);
            var proposalStatus = proposalCoordinator.GetStatus();

            var response = new BackgroundWorkStatusResponse
            {
                QueueEnabled = queueOptions.Enabled,
                PendingConversationCount = queueSnapshot.PendingConversationCount,
                PendingItems = queueSnapshot.PendingItems
                    .Select(x => new BackgroundWorkPendingItemResponse
                    {
                        ConversationId = x.ConversationId,
                        LastReason = x.LastReason,
                        LastScheduledAt = x.LastScheduledAt,
                        RetrievalDueAt = x.RetrievalDueAt,
                        MemoryDueAt = x.MemoryDueAt,
                        SummaryDueAt = x.SummaryDueAt,
                        RetrievalDueNow = x.RetrievalDueNow,
                        MemoryDueNow = x.MemoryDueNow,
                        SummaryDueNow = x.SummaryDueNow
                    })
                    .ToList(),
                BackgroundProposalSweepEnabled = proposalStatus.Enabled,
                BackgroundProposalSweepRunning = proposalStatus.IsSweepRunning,
                LastSweepStartedAt = proposalStatus.LastSweepStartedAt,
                LastSweepCompletedAt = proposalStatus.LastSweepCompletedAt,
                LastSweepMessage = proposalStatus.LastSweepMessage,
                LastSweepTriggeredConversationCount = proposalStatus.LastSweepTriggeredConversationCount
            };

            return Results.Ok(response);
        });

        group.MapPost("/conversations/{conversationId:guid}/refresh-summary", async (
            Guid conversationId,
            IConversationSummaryRefreshService summaryRefreshService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await summaryRefreshService.RefreshAsync(conversationId, cancellationToken);

                return Results.Ok(new BackgroundWorkManualTriggerResponse
                {
                    ConversationId = conversationId,
                    Operation = "RefreshSummary",
                    Succeeded = true,
                    Message = result.Reason,
                    SummaryRefreshed = result.Refreshed,
                    SummaryStartSequenceNumber = result.StartSequenceNumber,
                    SummaryEndSequenceNumber = result.EndSequenceNumber
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new BackgroundWorkManualTriggerResponse
                {
                    ConversationId = conversationId,
                    Operation = "RefreshSummary",
                    Succeeded = false,
                    Message = ex.Message
                });
            }
        });

        group.MapPost("/conversations/{conversationId:guid}/extract-memory", async (
            Guid conversationId,
            IMemoryProposalService proposalService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await proposalService.GenerateForConversationAsync(conversationId, cancellationToken);

                return Results.Ok(new BackgroundWorkManualTriggerResponse
                {
                    ConversationId = conversationId,
                    Operation = "ExtractMemory",
                    Succeeded = true,
                    Message = "Memory extraction completed.",
                    AttemptedCandidates = result.AttemptedCandidates,
                    CreatedProposalCount = result.CreatedProposalCount,
                    AutoSavedSceneStateCount = result.AutoSavedSceneStateCount,
                    AutoAcceptedDurableCount = result.AutoAcceptedDurableCount
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new BackgroundWorkManualTriggerResponse
                {
                    ConversationId = conversationId,
                    Operation = "ExtractMemory",
                    Succeeded = false,
                    Message = ex.Message
                });
            }
        });

        group.MapPost("/conversations/{conversationId:guid}/reindex-retrieval", async (
            Guid conversationId,
            IConversationRetrievalSyncService retrievalSyncService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await retrievalSyncService.ReindexConversationAsync(conversationId, cancellationToken);

                return Results.Ok(new BackgroundWorkManualTriggerResponse
                {
                    ConversationId = conversationId,
                    Operation = "ReindexRetrieval",
                    Succeeded = true,
                    Message = "Retrieval reindex completed.",
                    RetrievalReindexed = true
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new BackgroundWorkManualTriggerResponse
                {
                    ConversationId = conversationId,
                    Operation = "ReindexRetrieval",
                    Succeeded = false,
                    Message = ex.Message,
                    RetrievalReindexed = false
                });
            }
        });

        return app;
    }
}
