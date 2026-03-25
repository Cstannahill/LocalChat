using LocalChat.Contracts.Admin;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Infrastructure.BackgroundJobs;

namespace LocalChat.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin");

        group.MapGet("/retrieval/stats", async (
            IRetrievalAdminService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetStatsAsync(cancellationToken);
            return Results.Ok(result);
        });

        group.MapPost("/retrieval/reindex/conversations/{conversationId:guid}", async (
            Guid conversationId,
            IRetrievalAdminService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ReindexConversationAsync(conversationId, cancellationToken);
            return Results.Ok(result);
        });

        group.MapPost("/retrieval/reindex/all", async (
            IRetrievalAdminService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ReindexAllAsync(cancellationToken);
            return Results.Ok(result);
        });

        group.MapGet("/memory-proposals/background/status", (
            BackgroundMemoryProposalCoordinator coordinator) =>
        {
            var result = coordinator.GetStatus();

            return Results.Ok(new BackgroundProposalStatusResponse
            {
                Enabled = result.Enabled,
                IsSweepRunning = result.IsSweepRunning,
                LastSweepStartedAt = result.LastSweepStartedAt,
                LastSweepCompletedAt = result.LastSweepCompletedAt,
                LastSweepMessage = result.LastSweepMessage,
                LastSweepTriggeredConversationCount = result.LastSweepTriggeredConversationCount,
                CooldownTrackedConversationCount = result.CooldownTrackedConversationCount
            });
        });

        group.MapPost("/memory-proposals/background/run/{conversationId:guid}", async (
            Guid conversationId,
            BackgroundMemoryProposalCoordinator coordinator,
            CancellationToken cancellationToken) =>
        {
            var result = await coordinator.RunConversationNowAsync(conversationId, cancellationToken);

            return Results.Ok(new BackgroundProposalRunResponse
            {
                Succeeded = result.Succeeded,
                ConversationId = result.ConversationId,
                Message = result.Message,
                AttemptedCandidates = result.AttemptedCandidates,
                CreatedProposalCount = result.CreatedProposalCount,
                SkippedLowConfidenceCount = result.SkippedLowConfidenceCount,
                SkippedDuplicateCount = result.SkippedDuplicateCount,
                ConflictAnnotatedCount = result.ConflictAnnotatedCount,
                InvalidCandidateCount = result.InvalidCandidateCount
            });
        });

        return app;
    }
}
