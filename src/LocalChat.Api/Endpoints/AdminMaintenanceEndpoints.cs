using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Features.Memory;
using LocalChat.Contracts.Admin;

namespace LocalChat.Api.Endpoints;

public static class AdminMaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapAdminMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/maintenance")
            .WithTags("Admin");

        group.MapPost("/memory/rebuild-keys", async (
            IMemoryMaintenanceService memoryMaintenanceService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await memoryMaintenanceService.RebuildKeysAsync(cancellationToken);

                return Results.Ok(new MemoryRepairKeysResponse
                {
                    Succeeded = true,
                    Message = "Memory keys rebuilt.",
                    ScannedCount = result.ScannedCount,
                    RebuiltNormalizedKeyCount = result.RebuiltNormalizedKeyCount,
                    RebuiltSlotKeyCount = result.RebuiltSlotKeyCount,
                    RebuiltSlotFamilyCount = result.RebuiltSlotFamilyCount,
                    UpdatedCount = result.UpdatedCount
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new MemoryRepairKeysResponse
                {
                    Succeeded = false,
                    Message = ex.Message,
                    ScannedCount = 0,
                    RebuiltNormalizedKeyCount = 0,
                    RebuiltSlotKeyCount = 0,
                    RebuiltSlotFamilyCount = 0,
                    UpdatedCount = 0
                });
            }
        });

        group.MapPost("/retrieval/reindex-all", async (
            IFullRetrievalReindexService retrievalReindexService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var count = await retrievalReindexService.ReindexAllAsync(cancellationToken);

                return Results.Ok(new RetrievalReindexAllResponse
                {
                    Succeeded = true,
                    Message = "Full retrieval reindex completed.",
                    ReindexedDocumentCount = count
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new RetrievalReindexAllResponse
                {
                    Succeeded = false,
                    Message = ex.Message,
                    ReindexedDocumentCount = 0
                });
            }
        });

        group.MapPost("/audit/memory-extraction/prune", async (
            int? olderThanDays,
            IMemoryExtractionAuditEventRepository auditRepository,
            CancellationToken cancellationToken) =>
        {
            var keepDays = olderThanDays.GetValueOrDefault(30);
            if (keepDays <= 0)
            {
                keepDays = 30;
            }

            try
            {
                var cutoff = DateTime.UtcNow.AddDays(-keepDays);
                var deleted = await auditRepository.DeleteOlderThanAsync(cutoff, cancellationToken);

                return Results.Ok(new MemoryExtractionAuditPruneResponse
                {
                    Succeeded = true,
                    Message = "Memory extraction audit prune completed.",
                    OlderThanDays = keepDays,
                    DeletedCount = deleted
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new MemoryExtractionAuditPruneResponse
                {
                    Succeeded = false,
                    Message = ex.Message,
                    OlderThanDays = keepDays,
                    DeletedCount = 0
                });
            }
        });

        group.MapGet("/audit/memory-extraction/export/conversations/{conversationId:guid}", async (
            Guid conversationId,
            int? maxCount,
            IMemoryExtractionAuditEventRepository auditRepository,
            CancellationToken cancellationToken) =>
        {
            var take = maxCount.GetValueOrDefault(250);
            if (take <= 0)
            {
                take = 250;
            }

            var events = await auditRepository.ListByConversationAsync(conversationId, take, cancellationToken);

            var payload = new
            {
                conversationId,
                exportedAt = DateTime.UtcNow,
                count = events.Count,
                events = events.Select(x => new
                {
                    x.Id,
                    Category = x.Category.ToString(),
                    Kind = x.Kind.ToString(),
                    SlotFamily = x.SlotFamily.ToString(),
                    x.SlotKey,
                    x.CandidateContent,
                    x.CandidateNormalizedKey,
                    x.ConfidenceScore,
                    x.Action,
                    x.ExistingMemoryItemId,
                    x.ExistingMemoryContent,
                    x.Notes,
                    x.CreatedAt
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(
                payload,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

            return Results.File(
                System.Text.Encoding.UTF8.GetBytes(json),
                "application/json",
                $"memory-extraction-audit-{conversationId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        });

        group.MapPost("/memory/prune-stale-scene-state", async (
            ISceneStateCleanupService cleanupService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await cleanupService.PruneStaleAsync(cancellationToken);

                return Results.Ok(new SceneStateCleanupResponse
                {
                    Succeeded = true,
                    Message = "Stale scene-state cleanup completed.",
                    ScannedCount = result.ScannedCount,
                    RemovedCount = result.RemovedCount,
                    RemovedByFamily = result.RemovedByFamily
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new SceneStateCleanupResponse
                {
                    Succeeded = false,
                    Message = ex.Message,
                    ScannedCount = 0,
                    RemovedCount = 0,
                    RemovedByFamily = new Dictionary<string, int>()
                });
            }
        });

        return app;
    }
}
