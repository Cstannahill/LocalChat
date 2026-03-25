
using System.Text;
using System.Text.Json;
using LocalChat.Application.Memory;
using LocalChat.Contracts.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class MemoryAdminEndpoints
{
    public static IEndpointRouteBuilder MapMemoryAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memory")
            .WithTags("Memory Admin");

        group.MapPost("/{memoryId:guid}/demote-to-conversation/{conversationId:guid}", async (
            Guid memoryId,
            Guid conversationId,
            ApplicationDbContext dbContext,
            IMemoryOperationAuditService memoryOperationAuditService,
            CancellationToken cancellationToken) =>
        {
            var memoryItem = await dbContext.MemoryItems.FirstOrDefaultAsync(x => x.Id == memoryId, cancellationToken);
            if (memoryItem is null)
            {
                return Results.NotFound();
            }

            var beforeSnapshot = MemoryAuditSnapshot.From(memoryItem);

            memoryItem.ScopeType = MemoryScopeType.Conversation;
            memoryItem.ConversationId = conversationId;
            memoryItem.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            await memoryOperationAuditService.RecordAsync(
                memoryItem.Id,
                MemoryOperationType.DemotedToConversation,
                beforeState: beforeSnapshot,
                afterState: MemoryAuditSnapshot.From(memoryItem),
                conversationId: conversationId,
                agentId: memoryItem.AgentId,
                note: "Demoted from agent scope to conversation scope",
                cancellationToken: cancellationToken);

            return Results.Ok();
        });

        group.MapPost("/merge-preview", async (
            MergeMemoryItemsRequest request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            if (!request.SourceMemoryId.HasValue || !request.TargetMemoryId.HasValue)
            {
                return Results.BadRequest("SourceMemoryId and TargetMemoryId are required.");
            }

            if (request.SourceMemoryId.Value == request.TargetMemoryId.Value)
            {
                return Results.BadRequest("Source and target memory ids must be different.");
            }

            var source = await dbContext.MemoryItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SourceMemoryId.Value, cancellationToken);

            var target = await dbContext.MemoryItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.TargetMemoryId.Value, cancellationToken);

            if (source is null || target is null)
            {
                return Results.NotFound();
            }

            var strategy = NormalizeMergeStrategy(request.Strategy);
            var mergedContent = strategy switch
            {
                "replace_target" => source.Content,
                _ => MergeContentAppendUnique(target.Content, source.Content)
            };

            return Results.Ok(new MergeMemoryPreviewResponse
            {
                SourceMemoryId = source.Id,
                TargetMemoryId = target.Id,
                Strategy = strategy,
                SourceContent = source.Content,
                TargetContent = target.Content,
                MergedContent = mergedContent,
                ContentWillChange = !string.Equals(target.Content, mergedContent, StringComparison.Ordinal),
                ResultNormalizedKey = string.IsNullOrWhiteSpace(target.NormalizedKey) ? source.NormalizedKey : target.NormalizedKey,
                ResultCategory = target.Category.ToString()
            });
        });

        group.MapPost("/merge", async (
            MergeMemoryItemsRequest request,
            ApplicationDbContext dbContext,
            IMemoryOperationAuditService memoryOperationAuditService,
            CancellationToken cancellationToken) =>
        {
            if (!request.SourceMemoryId.HasValue || !request.TargetMemoryId.HasValue)
            {
                return Results.BadRequest("SourceMemoryId and TargetMemoryId are required.");
            }

            if (request.SourceMemoryId.Value == request.TargetMemoryId.Value)
            {
                return Results.BadRequest("Source and target memory ids must be different.");
            }

            var source = await dbContext.MemoryItems.FirstOrDefaultAsync(x => x.Id == request.SourceMemoryId.Value, cancellationToken);
            var target = await dbContext.MemoryItems.FirstOrDefaultAsync(x => x.Id == request.TargetMemoryId.Value, cancellationToken);

            if (source is null || target is null)
            {
                return Results.NotFound();
            }

            if (source.Kind != MemoryKind.DurableFact || target.Kind != MemoryKind.DurableFact)
            {
                return Results.BadRequest("Manual merge/upsert currently supports durable memory only.");
            }

            var sourceBefore = MemoryAuditSnapshot.From(source);
            var targetBefore = MemoryAuditSnapshot.From(target);

            var strategy = NormalizeMergeStrategy(request.Strategy);
            ApplyMergeToTarget(target, source, strategy);

            dbContext.MemoryItems.Remove(source);
            await dbContext.SaveChangesAsync(cancellationToken);

            await memoryOperationAuditService.RecordAsync(
                memoryItemId: target.Id,
                operationType: MemoryOperationType.MergedIntoTarget,
                beforeState: new MemoryMergeAuditSnapshot
                {
                    Source = sourceBefore,
                    Target = targetBefore
                },
                afterState: MemoryAuditSnapshot.From(target),
                sourceMemoryItemId: source.Id,
                targetMemoryItemId: target.Id,
                conversationId: target.ConversationId,
                agentId: target.AgentId,
                note: $"Merged source memory {source.Id} into target memory {target.Id}",
                cancellationToken: cancellationToken);

            return Results.Ok(new
            {
                mergedIntoMemoryId = target.Id,
                strategy
            });
        });

        group.MapGet("/conflicts", async (
            Guid? conversationId,
            Guid? agentId,
            int? maxCount,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var suggestions = await BuildConflictSuggestionsAsync(
                dbContext,
                conversationId,
                agentId,
                maxCount,
                cancellationToken);

            return Results.Ok(suggestions);
        });

        group.MapPost("/conflicts/resolve-bulk", async (
            BulkResolveMemoryConflictsRequest request,
            ApplicationDbContext dbContext,
            IMemoryOperationAuditService memoryOperationAuditService,
            CancellationToken cancellationToken) =>
        {
            var strategy = NormalizeMergeStrategy(request.Strategy);

            var suggestions = await BuildConflictSuggestionsAsync(
                dbContext,
                request.ConversationId,
                request.AgentId,
                request.MaxCount,
                cancellationToken);

            var results = new List<BulkResolvedMemoryConflictItemResponse>();

            foreach (var suggestion in suggestions)
            {
                var source = await dbContext.MemoryItems
                    .FirstOrDefaultAsync(x => x.Id == suggestion.SourceMemoryId, cancellationToken);

                var target = await dbContext.MemoryItems
                    .FirstOrDefaultAsync(x => x.Id == suggestion.TargetMemoryId, cancellationToken);

                if (source is null || target is null)
                {
                    results.Add(new BulkResolvedMemoryConflictItemResponse
                    {
                        SourceMemoryId = suggestion.SourceMemoryId,
                        TargetMemoryId = suggestion.TargetMemoryId,
                        Strategy = strategy,
                        Status = "skipped",
                        Message = "Source or target no longer exists."
                    });
                    continue;
                }

                if (source.Kind != MemoryKind.DurableFact || target.Kind != MemoryKind.DurableFact)
                {
                    results.Add(new BulkResolvedMemoryConflictItemResponse
                    {
                        SourceMemoryId = suggestion.SourceMemoryId,
                        TargetMemoryId = suggestion.TargetMemoryId,
                        Strategy = strategy,
                        Status = "skipped",
                        Message = "Only durable memory supports bulk merge."
                    });
                    continue;
                }

                var sourceBefore = MemoryAuditSnapshot.From(source);
                var targetBefore = MemoryAuditSnapshot.From(target);
                ApplyMergeToTarget(target, source, strategy);
                dbContext.MemoryItems.Remove(source);

                results.Add(new BulkResolvedMemoryConflictItemResponse
                {
                    SourceMemoryId = suggestion.SourceMemoryId,
                    TargetMemoryId = suggestion.TargetMemoryId,
                    Strategy = strategy,
                    Status = "merged",
                    Message = null
                });

                await memoryOperationAuditService.RecordAsync(
                    memoryItemId: target.Id,
                    operationType: MemoryOperationType.MergedIntoTarget,
                    beforeState: new MemoryMergeAuditSnapshot
                    {
                        Source = sourceBefore,
                        Target = targetBefore
                    },
                    afterState: MemoryAuditSnapshot.From(target),
                    sourceMemoryItemId: source.Id,
                    targetMemoryItemId: target.Id,
                    conversationId: target.ConversationId,
                    agentId: target.AgentId,
                    note: $"Bulk-merged source memory {source.Id} into target memory {target.Id}",
                    cancellationToken: cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new BulkResolveMemoryConflictsResponse
            {
                ScannedConflictCount = suggestions.Count,
                MergedCount = results.Count(x => x.Status == "merged"),
                SkippedCount = results.Count(x => x.Status == "skipped"),
                Items = results
            });
        });

        group.MapGet("/{memoryId:guid}/provenance", async (
            Guid memoryId,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var memoryItem = await dbContext.MemoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == memoryId, cancellationToken);

            if (memoryItem is null)
            {
                return Results.NotFound();
            }

            var audits = await dbContext.MemoryOperationAudits
                .AsNoTracking()
                .Where(x =>
                    x.MemoryItemId == memoryId ||
                    x.SourceMemoryItemId == memoryId ||
                    x.TargetMemoryItemId == memoryId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return Results.Ok(new MemoryProvenanceResponse
            {
                Id = memoryItem.Id,
                ScopeType = memoryItem.ScopeType.ToString(),
                Kind = memoryItem.Kind.ToString(),
                ConversationId = memoryItem.ConversationId,
                AgentId = memoryItem.AgentId,
                Category = memoryItem.Category.ToString(),
                Content = memoryItem.Content,
                NormalizedKey = memoryItem.NormalizedKey,
                SlotKey = memoryItem.SlotKey,
                SceneFamily = memoryItem.SlotFamily.ToString(),
                SourceMessageSequenceNumber = memoryItem.SourceMessageSequenceNumber,
                LastObservedSequenceNumber = memoryItem.LastObservedSequenceNumber,
                SupersededAtSequenceNumber = memoryItem.SupersededAtSequenceNumber,
                CreatedAt = memoryItem.CreatedAt,
                UpdatedAt = memoryItem.UpdatedAt,
                AuditEntries = audits.Select(ToAuditEntryResponse).ToList()
            });
        });

        group.MapPost("/operations/{auditId:guid}/undo", async (
            Guid auditId,
            ApplicationDbContext dbContext,
            IMemoryOperationAuditService memoryOperationAuditService,
            CancellationToken cancellationToken) =>
        {
            var audit = await dbContext.MemoryOperationAudits.FirstOrDefaultAsync(x => x.Id == auditId, cancellationToken);
            if (audit is null)
            {
                return Results.NotFound();
            }

            if (audit.IsUndone)
            {
                return Results.BadRequest("This operation has already been undone.");
            }

            switch (audit.OperationType)
            {
                case MemoryOperationType.PromotedToAgent:
                case MemoryOperationType.DemotedToConversation:
                {
                    if (string.IsNullOrWhiteSpace(audit.BeforeStateJson))
                    {
                        return Results.BadRequest("Undo snapshot is missing.");
                    }

                    var before = JsonSerializer.Deserialize<MemoryAuditSnapshot>(audit.BeforeStateJson);
                    if (before is null)
                    {
                        return Results.BadRequest("Undo snapshot could not be read.");
                    }

                    var memoryItem = await dbContext.MemoryItems.FirstOrDefaultAsync(x => x.Id == audit.MemoryItemId, cancellationToken);
                    if (memoryItem is null)
                    {
                        return Results.NotFound("Target memory item no longer exists.");
                    }

                    before.ApplyTo(memoryItem);
                    memoryItem.UpdatedAt = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync(cancellationToken);

                    var undoAuditId = await memoryOperationAuditService.RecordAsync(
                        memoryItem.Id,
                        MemoryOperationType.UndoApplied,
                        beforeState: audit.AfterStateJson,
                        afterState: MemoryAuditSnapshot.From(memoryItem),
                        conversationId: memoryItem.ConversationId,
                        agentId: memoryItem.AgentId,
                        note: $"Undo of {audit.OperationType}",
                        cancellationToken: cancellationToken);

                    audit.IsUndone = true;
                    audit.UndoneAtUtc = DateTime.UtcNow;
                    audit.UndoAuditId = undoAuditId;
                    await dbContext.SaveChangesAsync(cancellationToken);

                    return Results.Ok();
                }

                case MemoryOperationType.MergedIntoTarget:
                {
                    if (string.IsNullOrWhiteSpace(audit.BeforeStateJson))
                    {
                        return Results.BadRequest("Undo snapshot is missing.");
                    }

                    var before = JsonSerializer.Deserialize<MemoryMergeAuditSnapshot>(audit.BeforeStateJson);
                    if (before is null)
                    {
                        return Results.BadRequest("Undo snapshot could not be read.");
                    }

                    var target = await dbContext.MemoryItems.FirstOrDefaultAsync(x => x.Id == before.Target.Id, cancellationToken);
                    if (target is null)
                    {
                        return Results.NotFound("Target memory item no longer exists.");
                    }

                    before.Target.ApplyTo(target);
                    target.UpdatedAt = DateTime.UtcNow;

                    var source = await dbContext.MemoryItems.FirstOrDefaultAsync(x => x.Id == before.Source.Id, cancellationToken);
                    if (source is null)
                    {
                        source = new MemoryItem
                        {
                            Id = before.Source.Id
                        };
                        before.Source.ApplyTo(source);
                        await dbContext.MemoryItems.AddAsync(source, cancellationToken);
                    }
                    else
                    {
                        before.Source.ApplyTo(source);
                    }

                    source.UpdatedAt = DateTime.UtcNow;

                    await dbContext.SaveChangesAsync(cancellationToken);

                    var undoAuditId = await memoryOperationAuditService.RecordAsync(
                        target.Id,
                        MemoryOperationType.UndoApplied,
                        beforeState: audit.AfterStateJson,
                        afterState: new MemoryMergeAuditSnapshot
                        {
                            Source = MemoryAuditSnapshot.From(source),
                            Target = MemoryAuditSnapshot.From(target)
                        },
                        sourceMemoryItemId: source.Id,
                        targetMemoryItemId: target.Id,
                        conversationId: target.ConversationId,
                        agentId: target.AgentId,
                        note: $"Undo of {audit.OperationType}",
                        cancellationToken: cancellationToken);

                    audit.IsUndone = true;
                    audit.UndoneAtUtc = DateTime.UtcNow;
                    audit.UndoAuditId = undoAuditId;
                    await dbContext.SaveChangesAsync(cancellationToken);

                    return Results.Ok();
                }

                default:
                    return Results.BadRequest("Undo is only supported for promote, demote, and merge operations.");
            }
        });

        group.MapPost("/import", async (
            IFormFile file,
            string? format,
            string? strategy,
            Guid? conversationIdOverride,
            Guid? agentIdOverride,
            ApplicationDbContext dbContext,
            IMemoryOperationAuditService memoryOperationAuditService,
            CancellationToken cancellationToken) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest("A non-empty import file is required.");
            }

            var normalizedFormat = NormalizeImportFormat(format, file.FileName);
            if (normalizedFormat is null)
            {
                return Results.BadRequest("format must be one of: json, jsonl, sharegpt.");
            }

            var normalizedStrategy = NormalizeImportStrategy(strategy);

            string payload;
            await using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                payload = await reader.ReadToEndAsync();
            }

            var rows = ParseImportRows(payload, normalizedFormat);
            var warnings = new List<string>();
            var importedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Content))
                {
                    skippedCount++;
                    warnings.Add("Skipped a row with empty content.");
                    continue;
                }

                var resolvedConversationId = conversationIdOverride ?? row.ConversationId;
                var resolvedAgentId = agentIdOverride ?? row.AgentId;

                if (!resolvedAgentId.HasValue)
                {
                    skippedCount++;
                    var preview = row.Content[..Math.Min(row.Content.Length, 40)];
                    warnings.Add($"Skipped memory without agent id. Content preview: {preview}");
                    continue;
                }

                if (row.ScopeType == MemoryScopeType.Conversation && !resolvedConversationId.HasValue)
                {
                    skippedCount++;
                    warnings.Add("Skipped conversation-scoped memory without conversation id.");
                    continue;
                }

                if (normalizedStrategy == "upsert_normalized_key" && !string.IsNullOrWhiteSpace(row.NormalizedKey))
                {
                    var existing = await FindUpsertTargetAsync(
                        dbContext,
                        row.ScopeType,
                        resolvedConversationId,
                        resolvedAgentId.Value,
                        row.Kind,
                        row.NormalizedKey!,
                        cancellationToken);

                    if (existing is not null)
                    {
                        var beforeSnapshot = MemoryAuditSnapshot.From(existing);
                        existing.Content = MergeContentAppendUnique(existing.Content, row.Content);
                        if (string.IsNullOrWhiteSpace(existing.SlotKey) && !string.IsNullOrWhiteSpace(row.SlotKey))
                        {
                            existing.SlotKey = row.SlotKey;
                        }

                        if (existing.SlotFamily == MemorySlotFamily.None && row.SlotFamily.HasValue)
                        {
                            existing.SlotFamily = row.SlotFamily.Value;
                        }

                        existing.SourceMessageSequenceNumber = MinNullableInt(
                            existing.SourceMessageSequenceNumber,
                            row.SourceMessageSequenceNumber);
                        existing.LastObservedSequenceNumber = MaxNullableInt(
                            existing.LastObservedSequenceNumber,
                            row.LastObservedSequenceNumber);
                        existing.SupersededAtSequenceNumber = MinNullableInt(
                            existing.SupersededAtSequenceNumber,
                            row.SupersededAtSequenceNumber);
                        existing.UpdatedAt = DateTime.UtcNow;

                        await memoryOperationAuditService.RecordAsync(
                            existing.Id,
                            MemoryOperationType.Imported,
                            beforeState: beforeSnapshot,
                            afterState: MemoryAuditSnapshot.From(existing),
                            conversationId: existing.ConversationId,
                            agentId: existing.AgentId,
                            note: "Memory import upsert",
                            cancellationToken: cancellationToken);
                        updatedCount++;
                        continue;
                    }
                }

                var created = new MemoryItem
                {
                    Id = Guid.NewGuid(),
                    ScopeType = row.ScopeType,
                    Kind = row.Kind,
                    ConversationId = row.ScopeType == MemoryScopeType.Conversation ? resolvedConversationId : null,
                    AgentId = resolvedAgentId.Value,
                    Category = row.Category,
                    Content = row.Content,
                    ReviewStatus = MemoryReviewStatus.Accepted,
                    IsPinned = false,
                    ConfidenceScore = null,
                    ProposalReason = null,
                    SourceExcerpt = null,
                    NormalizedKey = row.NormalizedKey,
                    SlotKey = row.SlotKey,
                    SlotFamily = row.SlotFamily ?? MemorySlotFamily.None,
                    ConflictsWithMemoryItemId = null,
                    SourceMessageSequenceNumber = row.SourceMessageSequenceNumber,
                    LastObservedSequenceNumber = row.LastObservedSequenceNumber,
                    SupersededAtSequenceNumber = row.SupersededAtSequenceNumber,
                    ExpiresAt = null,
                    CreatedAt = row.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = row.UpdatedAt ?? DateTime.UtcNow
                };

                await dbContext.MemoryItems.AddAsync(created, cancellationToken);

                await memoryOperationAuditService.RecordAsync(
                    created.Id,
                    MemoryOperationType.Imported,
                    beforeState: null,
                    afterState: MemoryAuditSnapshot.From(created),
                    conversationId: created.ConversationId,
                    agentId: created.AgentId,
                    note: "Memory import insert",
                    cancellationToken: cancellationToken);
                importedCount++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new MemoryImportResponse
            {
                Format = normalizedFormat,
                Strategy = normalizedStrategy,
                ImportedCount = importedCount,
                UpdatedCount = updatedCount,
                SkippedCount = skippedCount,
                Warnings = warnings
            });
        });

        group.MapGet("/export", async (
            Guid? conversationId,
            Guid? agentId,
            string? scope,
            string? kind,
            string? categoryContains,
            DateTime? createdFromUtc,
            DateTime? createdToUtc,
            DateTime? updatedFromUtc,
            DateTime? updatedToUtc,
            bool? activeOnly,
            int? maxCount,
            string? format,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var take = maxCount.GetValueOrDefault(500);
            if (take <= 0)
            {
                take = 500;
            }

            if (take > 5000)
            {
                take = 5000;
            }

            var normalizedFormat = NormalizeExportFormat(format);
            if (normalizedFormat is null)
            {
                return Results.BadRequest("format must be one of: json, jsonl, sharegpt.");
            }

            MemoryScopeType? scopeFilter = null;
            if (!string.IsNullOrWhiteSpace(scope))
            {
                if (!Enum.TryParse<MemoryScopeType>(scope, true, out var parsedScope))
                {
                    return Results.BadRequest("Invalid scope.");
                }

                scopeFilter = parsedScope;
            }

            MemoryKind? kindFilter = null;
            if (!string.IsNullOrWhiteSpace(kind))
            {
                if (!TryParseMemoryKind(kind, out var parsedKind))
                {
                    return Results.BadRequest("Invalid kind.");
                }

                kindFilter = parsedKind;
            }

            var normalizedCategory = string.IsNullOrWhiteSpace(categoryContains)
                ? null
                : categoryContains.Trim().ToLowerInvariant();

            var query = dbContext.MemoryItems
                .AsNoTracking()
                .AsQueryable();

            if (conversationId.HasValue)
            {
                query = query.Where(x => x.ConversationId == conversationId.Value);
            }

            if (agentId.HasValue)
            {
                query = query.Where(x => x.AgentId == agentId.Value);
            }

            if (scopeFilter.HasValue)
            {
                query = query.Where(x => x.ScopeType == scopeFilter.Value);
            }

            if (kindFilter.HasValue)
            {
                query = query.Where(x => x.Kind == kindFilter.Value);
            }

            if (createdFromUtc.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= createdFromUtc.Value);
            }

            if (createdToUtc.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= createdToUtc.Value);
            }

            if (updatedFromUtc.HasValue)
            {
                query = query.Where(x => x.UpdatedAt >= updatedFromUtc.Value);
            }

            if (updatedToUtc.HasValue)
            {
                query = query.Where(x => x.UpdatedAt <= updatedToUtc.Value);
            }

            if (activeOnly.GetValueOrDefault(false))
            {
                query = query.Where(x => x.SupersededAtSequenceNumber == null);
            }

            var rows = await query
                .OrderByDescending(x => x.UpdatedAt)
                .Take(take * 2)
                .ToListAsync(cancellationToken);

            var mappedRows = rows
                .Select(x => new MemoryExportRow(
                    x.Id,
                    x.ScopeType.ToString(),
                    x.Kind.ToString(),
                    x.ConversationId,
                    x.AgentId,
                    x.Category.ToString(),
                    x.Content,
                    x.NormalizedKey,
                    x.SlotKey,
                    x.SlotFamily.ToString(),
                    x.SourceMessageSequenceNumber,
                    x.LastObservedSequenceNumber,
                    x.SupersededAtSequenceNumber,
                    x.CreatedAt,
                    x.UpdatedAt))
                .Where(x => string.IsNullOrWhiteSpace(normalizedCategory) ||
                            (x.Category?.ToLowerInvariant().Contains(normalizedCategory) ?? false))
                .Take(take)
                .ToList();

            return normalizedFormat switch
            {
                "json" => BuildJsonResult(
                    mappedRows,
                    new
                    {
                        conversationId,
                        agentId,
                        scope = scopeFilter?.ToString(),
                        kind = kindFilter?.ToString(),
                        categoryContains,
                        createdFromUtc,
                        createdToUtc,
                        updatedFromUtc,
                        updatedToUtc,
                        activeOnly = activeOnly.GetValueOrDefault(false),
                        maxCount = take,
                        format = normalizedFormat
                    }),
                "jsonl" => BuildJsonlResult(mappedRows),
                "sharegpt" => BuildShareGptMemoryAnnotationResult(mappedRows),
                _ => Results.BadRequest("Unsupported format.")
            };
        });

        return app;
    }

    private static async Task<List<MemoryConflictSuggestionResponse>> BuildConflictSuggestionsAsync(
        ApplicationDbContext dbContext,
        Guid? conversationId,
        Guid? agentId,
        int? maxCount,
        CancellationToken cancellationToken)
    {
        var take = maxCount.GetValueOrDefault(50);
        if (take <= 0)
        {
            take = 50;
        }

        if (take > 500)
        {
            take = 500;
        }

        var query = dbContext.MemoryItems
            .AsNoTracking()
            .Where(x => x.Kind == MemoryKind.DurableFact)
            .Where(x => x.SupersededAtSequenceNumber == null)
            .Where(x => !string.IsNullOrWhiteSpace(x.NormalizedKey));

        if (conversationId.HasValue)
        {
            query = query.Where(x =>
                (x.ScopeType == MemoryScopeType.Conversation && x.ConversationId == conversationId.Value) ||
                x.ScopeType == MemoryScopeType.Agent);
        }

        if (agentId.HasValue)
        {
            query = query.Where(x => x.AgentId == agentId.Value);
        }

        var rows = await query
            .OrderBy(x => x.NormalizedKey)
            .ThenBy(x => x.CreatedAt)
            .Take(2000)
            .ToListAsync(cancellationToken);

        var suggestions = rows
            .GroupBy(x => new
            {
                Key = (x.NormalizedKey ?? string.Empty).Trim().ToLowerInvariant(),
                x.AgentId
            })
            .Where(g => g.Count() > 1)
            .SelectMany(g =>
            {
                var ordered = g
                    .OrderBy(x => x.ScopeType == MemoryScopeType.Conversation ? 0 : 1)
                    .ThenBy(x => x.CreatedAt)
                    .ToList();

                return ordered
                    .Select(source =>
                    {
                        var candidates = ordered
                            .Where(x => x.Id != source.Id)
                            .Select(target =>
                            {
                                var (score, explanation) = ScoreConflictTarget(source, target);
                                return new
                                {
                                    Target = target,
                                    Score = score,
                                    Explanation = explanation
                                };
                            })
                            .OrderByDescending(x => x.Score)
                            .ToList();

                        var best = candidates.FirstOrDefault();
                        if (best is null)
                        {
                            return null;
                        }

                        return new MemoryConflictSuggestionResponse
                        {
                            SourceMemoryId = source.Id,
                            TargetMemoryId = best.Target.Id,
                            SuggestedStrategy = "append_unique",
                            Reason = BuildConflictReason(source, best.Target),
                            TargetScore = best.Score,
                            RankingExplanation = best.Explanation,
                            Source = ToSummary(source),
                            Target = ToSummary(best.Target)
                        };
                    })
                    .OfType<MemoryConflictSuggestionResponse>();
            })
            .OrderByDescending(x => x.Source.UpdatedAt)
            .Take(take)
            .ToList();

        return suggestions;
    }

    private static void ApplyMergeToTarget(MemoryItem target, MemoryItem source, string strategy)
    {
        target.Content = strategy switch
        {
            "replace_target" => source.Content,
            _ => MergeContentAppendUnique(target.Content, source.Content)
        };

        if (string.IsNullOrWhiteSpace(target.NormalizedKey) && !string.IsNullOrWhiteSpace(source.NormalizedKey))
        {
            target.NormalizedKey = source.NormalizedKey;
        }

        if (string.IsNullOrWhiteSpace(target.SlotKey) && !string.IsNullOrWhiteSpace(source.SlotKey))
        {
            target.SlotKey = source.SlotKey;
        }

        if (target.SlotFamily == MemorySlotFamily.None && source.SlotFamily != MemorySlotFamily.None)
        {
            target.SlotFamily = source.SlotFamily;
        }

        target.SourceMessageSequenceNumber = MinNullableInt(
            target.SourceMessageSequenceNumber,
            source.SourceMessageSequenceNumber);

        target.LastObservedSequenceNumber = MaxNullableInt(
            target.LastObservedSequenceNumber,
            source.LastObservedSequenceNumber);

        target.SupersededAtSequenceNumber = MinNullableInt(
            target.SupersededAtSequenceNumber,
            source.SupersededAtSequenceNumber);

        target.UpdatedAt = DateTime.UtcNow;
    }

    private static async Task<MemoryItem?> FindUpsertTargetAsync(
        ApplicationDbContext dbContext,
        MemoryScopeType scopeType,
        Guid? conversationId,
        Guid agentId,
        MemoryKind kind,
        string normalizedKey,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MemoryItems
            .Where(x => x.Kind == kind)
            .Where(x => x.ScopeType == scopeType)
            .Where(x => x.AgentId == agentId)
            .Where(x => x.NormalizedKey == normalizedKey);

        if (scopeType == MemoryScopeType.Conversation)
        {
            query = query.Where(x => x.ConversationId == conversationId);
        }
        else
        {
            query = query.Where(x => x.ConversationId == null);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private static List<MemoryImportRow> ParseImportRows(string payload, string format)
    {
        return format switch
        {
            "json" => ParseJsonImportRows(payload),
            "jsonl" => ParseJsonlImportRows(payload),
            "sharegpt" => ParseShareGptImportRows(payload),
            _ => new List<MemoryImportRow>()
        };
    }

    private static List<MemoryImportRow> ParseJsonImportRows(string payload)
    {
        using var doc = JsonDocument.Parse(payload);

        if (doc.RootElement.ValueKind == JsonValueKind.Object &&
            doc.RootElement.TryGetProperty("entries", out var entriesElement) &&
            entriesElement.ValueKind == JsonValueKind.Array)
        {
            return entriesElement.EnumerateArray()
                .Select(ParseJsonRow)
                .Where(x => x is not null)
                .Cast<MemoryImportRow>()
                .ToList();
        }

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            return doc.RootElement.EnumerateArray()
                .Select(ParseJsonRow)
                .Where(x => x is not null)
                .Cast<MemoryImportRow>()
                .ToList();
        }

        return new List<MemoryImportRow>();
    }

    private static List<MemoryImportRow> ParseJsonlImportRows(string payload)
    {
        var rows = new List<MemoryImportRow>();

        foreach (var line in payload.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                var row = ParseJsonRow(doc.RootElement);
                if (row is not null)
                {
                    rows.Add(row);
                }
            }
            catch
            {
                // Ignore malformed JSONL rows.
            }
        }

        return rows;
    }

    private static List<MemoryImportRow> ParseShareGptImportRows(string payload)
    {
        using var doc = JsonDocument.Parse(payload);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            return new List<MemoryImportRow>();
        }

        var rows = new List<MemoryImportRow>();

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("memory_annotations", out var annotations) ||
                annotations.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var annotation in annotations.EnumerateArray())
            {
                var row = ParseJsonRow(annotation);
                if (row is not null)
                {
                    rows.Add(row);
                }
            }
        }

        return rows;
    }

    private static MemoryImportRow? ParseJsonRow(JsonElement element)
    {
        var scopeText = ReadString(element, "scope") ?? ReadString(element, "scopeType");
        var kindText = ReadString(element, "kind");
        var content = ReadString(element, "content");

        if (string.IsNullOrWhiteSpace(scopeText) ||
            string.IsNullOrWhiteSpace(kindText) ||
            string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        if (!Enum.TryParse<MemoryScopeType>(scopeText, true, out var scopeType))
        {
            return null;
        }

        if (!TryParseMemoryKind(kindText, out var kind))
        {
            return null;
        }

        var slotFamilyText = ReadString(element, "slotFamily") ?? ReadString(element, "sceneFamily");
        MemorySlotFamily? slotFamily = null;
        if (!string.IsNullOrWhiteSpace(slotFamilyText) &&
            Enum.TryParse<MemorySlotFamily>(slotFamilyText, true, out var parsedSlotFamily))
        {
            slotFamily = parsedSlotFamily;
        }

        var categoryText = ReadString(element, "category");
        var category = ResolveCategory(categoryText, kind);

        return new MemoryImportRow(
            ScopeType: scopeType,
            Kind: kind,
            ConversationId: ReadGuid(element, "conversationId"),
            AgentId: ReadGuid(element, "agentId"),
            Category: category,
            Content: content,
            NormalizedKey: ReadString(element, "normalizedKey"),
            SlotKey: ReadString(element, "slotKey"),
            SlotFamily: slotFamily,
            SourceMessageSequenceNumber: ReadInt(element, "sourceMessageSequenceNumber"),
            LastObservedSequenceNumber: ReadInt(element, "lastObservedSequenceNumber"),
            SupersededAtSequenceNumber: ReadInt(element, "supersededAtSequenceNumber"),
            CreatedAt: ReadDateTime(element, "createdAt"),
            UpdatedAt: ReadDateTime(element, "updatedAt"));
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static Guid? ReadGuid(JsonElement element, string propertyName)
    {
        var text = ReadString(element, propertyName);
        return Guid.TryParse(text, out var parsed) ? parsed : null;
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed))
        {
            return parsed;
        }

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out parsed))
        {
            return parsed;
        }

        return null;
    }

    private static DateTime? ReadDateTime(JsonElement element, string propertyName)
    {
        var text = ReadString(element, propertyName);
        return DateTime.TryParse(text, out var parsed) ? parsed : null;
    }

    private static bool TryParseMemoryKind(string? kindText, out MemoryKind kind)
    {
        kind = MemoryKind.DurableFact;
        if (string.IsNullOrWhiteSpace(kindText))
        {
            return false;
        }

        var normalized = kindText.Trim();
        if (normalized.Equals("Durable", StringComparison.OrdinalIgnoreCase))
        {
            kind = MemoryKind.DurableFact;
            return true;
        }

        return Enum.TryParse(normalized, true, out kind);
    }

    private static MemoryCategory ResolveCategory(string? categoryText, MemoryKind kind)
    {
        if (!string.IsNullOrWhiteSpace(categoryText) &&
            Enum.TryParse<MemoryCategory>(categoryText, true, out var parsed))
        {
            return parsed;
        }

        return kind == MemoryKind.SessionState
            ? MemoryCategory.SessionState
            : MemoryCategory.UserFact;
    }

    private static string BuildConflictReason(MemoryItem source, MemoryItem target)
    {
        if (source.ScopeType == target.ScopeType)
        {
            return "Same normalized key in the same effective memory scope.";
        }

        return "Same normalized key across overlapping memory scopes.";
    }

    private static MemoryConflictMemorySummaryResponse ToSummary(MemoryItem memoryItem)
    {
        return new MemoryConflictMemorySummaryResponse
        {
            Id = memoryItem.Id,
            ScopeType = memoryItem.ScopeType.ToString(),
            Kind = memoryItem.Kind.ToString(),
            Category = memoryItem.Category.ToString(),
            NormalizedKey = memoryItem.NormalizedKey,
            Content = memoryItem.Content,
            ConversationId = memoryItem.ConversationId,
            AgentId = memoryItem.AgentId,
            SourceMessageSequenceNumber = memoryItem.SourceMessageSequenceNumber,
            LastObservedSequenceNumber = memoryItem.LastObservedSequenceNumber,
            CreatedAt = memoryItem.CreatedAt,
            UpdatedAt = memoryItem.UpdatedAt
        };
    }

    private static MemoryOperationAuditEntryResponse ToAuditEntryResponse(MemoryOperationAudit audit)
    {
        return new MemoryOperationAuditEntryResponse
        {
            Id = audit.Id,
            OperationType = audit.OperationType.ToString(),
            MessageSequenceNumber = audit.MessageSequenceNumber,
            Note = audit.Note,
            IsUndone = audit.IsUndone,
            CanUndo = !audit.IsUndone && (
                audit.OperationType == MemoryOperationType.PromotedToAgent ||
                audit.OperationType == MemoryOperationType.DemotedToConversation ||
                audit.OperationType == MemoryOperationType.MergedIntoTarget),
            CreatedAt = audit.CreatedAt,
            BeforeContentPreview = ExtractContentPreview(audit.BeforeStateJson),
            AfterContentPreview = ExtractContentPreview(audit.AfterStateJson)
        };
    }

    private static string? ExtractContentPreview(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("Content", out var directContent) &&
                directContent.ValueKind == JsonValueKind.String)
            {
                return directContent.GetString();
            }

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("Target", out var target) &&
                target.ValueKind == JsonValueKind.Object &&
                target.TryGetProperty("Content", out var targetContent) &&
                targetContent.ValueKind == JsonValueKind.String)
            {
                return targetContent.GetString();
            }
        }
        catch
        {
            // Ignore parse failures.
        }

        return null;
    }

    private static (double Score, IReadOnlyList<string> Explanation) ScoreConflictTarget(
        MemoryItem source,
        MemoryItem target)
    {
        var score = 0.0;
        var explanation = new List<string>();

        if (source.ScopeType == MemoryScopeType.Conversation &&
            target.ScopeType == MemoryScopeType.Conversation &&
            source.ConversationId.HasValue &&
            target.ConversationId == source.ConversationId)
        {
            score += 1.0;
            explanation.Add("Same conversation scope.");
        }

        if (target.ScopeType == source.ScopeType)
        {
            score += 0.45;
            explanation.Add("Same memory scope.");
        }

        if (target.Category != default)
        {
            score += 0.15;
            explanation.Add("Has category metadata.");
        }

        if (!string.IsNullOrWhiteSpace(target.NormalizedKey))
        {
            score += 0.15;
            explanation.Add("Has normalized key.");
        }

        if ((target.Content?.Length ?? 0) >= (source.Content?.Length ?? 0))
        {
            score += 0.2;
            explanation.Add("Target content is at least as rich as source content.");
        }

        if (target.CreatedAt <= source.CreatedAt)
        {
            score += 0.1;
            explanation.Add("Older target keeps history stable.");
        }

        return (score, explanation);
    }

    private static IResult BuildJsonResult(
        IReadOnlyList<MemoryExportRow> rows,
        object filters)
    {
        var payload = JsonSerializer.Serialize(
            new
            {
                exportedAt = DateTime.UtcNow,
                filters,
                count = rows.Count,
                entries = rows
            },
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var bytes = Encoding.UTF8.GetBytes(payload);

        return Results.File(
            bytes,
            "application/json",
            $"memory-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    private static IResult BuildJsonlResult(IReadOnlyList<MemoryExportRow> rows)
    {
        var lines = rows.Select(x => JsonSerializer.Serialize(x));
        var jsonl = string.Join('\n', lines);
        var bytes = Encoding.UTF8.GetBytes(jsonl);

        return Results.File(
            bytes,
            "application/x-ndjson",
            $"memory-export-{DateTime.UtcNow:yyyyMMddHHmmss}.jsonl");
    }

    private static IResult BuildShareGptMemoryAnnotationResult(IReadOnlyList<MemoryExportRow> rows)
    {
        var payload = rows.Select(x => new
        {
            id = x.Id,
            conversations = Array.Empty<object>(),
            memory_annotations = new object[]
            {
                new
                {
                    scope = x.Scope,
                    kind = x.Kind,
                    x.ConversationId,
                    x.AgentId,
                    x.Category,
                    x.Content,
                    x.NormalizedKey,
                    x.SlotKey,
                    slotFamily = x.SlotFamily,
                    x.SourceMessageSequenceNumber,
                    x.LastObservedSequenceNumber,
                    x.SupersededAtSequenceNumber,
                    x.CreatedAt,
                    x.UpdatedAt
                }
            }
        }).ToList();

        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var bytes = Encoding.UTF8.GetBytes(json);

        return Results.File(
            bytes,
            "application/json",
            $"memory-export-sharegpt-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    private static string MergeContentAppendUnique(string target, string source)
    {
        var left = (target ?? string.Empty).Trim();
        var right = (source ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(left))
        {
            return right;
        }

        if (string.IsNullOrWhiteSpace(right))
        {
            return left;
        }

        if (left.Contains(right, StringComparison.OrdinalIgnoreCase))
        {
            return left;
        }

        if (right.Contains(left, StringComparison.OrdinalIgnoreCase))
        {
            return right;
        }

        return $"{left}\n{right}";
    }

    private static int? MinNullableInt(int? a, int? b)
    {
        if (!a.HasValue)
        {
            return b;
        }

        if (!b.HasValue)
        {
            return a;
        }

        return Math.Min(a.Value, b.Value);
    }

    private static int? MaxNullableInt(int? a, int? b)
    {
        if (!a.HasValue)
        {
            return b;
        }

        if (!b.HasValue)
        {
            return a;
        }

        return Math.Max(a.Value, b.Value);
    }

    private static string NormalizeMergeStrategy(string? strategy)
    {
        var value = string.IsNullOrWhiteSpace(strategy)
            ? "append_unique"
            : strategy.Trim().ToLowerInvariant();

        return value switch
        {
            "replace_target" => "replace_target",
            _ => "append_unique"
        };
    }

    private static string? NormalizeExportFormat(string? format)
    {
        var value = string.IsNullOrWhiteSpace(format)
            ? "json"
            : format.Trim().ToLowerInvariant();

        return value switch
        {
            "json" => "json",
            "jsonl" => "jsonl",
            "sharegpt" => "sharegpt",
            _ => null
        };
    }

    private static string? NormalizeImportFormat(string? format, string? fileName)
    {
        var value = string.IsNullOrWhiteSpace(format)
            ? string.Empty
            : format.Trim().ToLowerInvariant();

        if (value is "json" or "jsonl" or "sharegpt")
        {
            return value;
        }

        var lowerName = (fileName ?? string.Empty).Trim().ToLowerInvariant();

        if (lowerName.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
        {
            return "jsonl";
        }

        if (lowerName.Contains("sharegpt", StringComparison.OrdinalIgnoreCase))
        {
            return "sharegpt";
        }

        if (lowerName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return "json";
        }

        return null;
    }

    private static string NormalizeImportStrategy(string? strategy)
    {
        var value = string.IsNullOrWhiteSpace(strategy)
            ? "upsert_normalized_key"
            : strategy.Trim().ToLowerInvariant();

        return value switch
        {
            "insert_only" => "insert_only",
            _ => "upsert_normalized_key"
        };
    }

    private sealed record MemoryExportRow(
        Guid Id,
        string Scope,
        string Kind,
        Guid? ConversationId,
        Guid? AgentId,
        string? Category,
        string Content,
        string? NormalizedKey,
        string? SlotKey,
        string SlotFamily,
        int? SourceMessageSequenceNumber,
        int? LastObservedSequenceNumber,
        int? SupersededAtSequenceNumber,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed record MemoryImportRow(
        MemoryScopeType ScopeType,
        MemoryKind Kind,
        Guid? ConversationId,
        Guid? AgentId,
        MemoryCategory Category,
        string Content,
        string? NormalizedKey,
        string? SlotKey,
        MemorySlotFamily? SlotFamily,
        int? SourceMessageSequenceNumber,
        int? LastObservedSequenceNumber,
        int? SupersededAtSequenceNumber,
        DateTime? CreatedAt,
        DateTime? UpdatedAt);
}
