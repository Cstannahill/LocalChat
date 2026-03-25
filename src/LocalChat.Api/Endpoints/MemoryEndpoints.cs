using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Features.Memory;
using LocalChat.Application.Memory;
using LocalChat.Contracts.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Api.Endpoints;

public static class MemoryEndpoints
{
    public static IEndpointRouteBuilder MapMemoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memory").WithTags("Memory");

        group.MapGet(
            "/by-character/{characterId:guid}",
            async (
                Guid characterId,
                Guid? conversationId,
                IMemoryRepository repository,
                CancellationToken cancellationToken
            ) =>
            {
                IReadOnlyList<MemoryItem> items = conversationId.HasValue
                    ? await repository.ListByConversationAsync(conversationId.Value, cancellationToken)
                    : await repository.ListByCharacterAsync(characterId, cancellationToken);

                items = items.Where(x => x.CharacterId == characterId).ToList();

                return Results.Ok(items.Select(ToResponse).ToList());
            }
        );

        group.MapGet(
            "/proposals/by-character/{characterId:guid}",
            async (
                Guid characterId,
                Guid? conversationId,
                IMemoryRepository repository,
                CancellationToken cancellationToken
            ) =>
            {
                IReadOnlyList<MemoryItem> items = conversationId.HasValue
                    ? await repository.ListByConversationAsync(conversationId.Value, cancellationToken)
                    : await repository.ListByCharacterAsync(characterId, cancellationToken);

                items = items
                    .Where(x => x.CharacterId == characterId && x.ReviewStatus == MemoryReviewStatus.Proposed)
                    .ToList();

                return Results.Ok(items.Select(ToResponse).ToList());
            }
        );

        group.MapPost(
            "/",
            async (
                CreateMemoryItemRequest request,
                ICharacterRepository characterRepository,
                IConversationRepository conversationRepository,
                IMemoryRepository memoryRepository,
                IRetrievalService retrievalService,
                CancellationToken cancellationToken
            ) =>
            {
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return Results.BadRequest(new { error = "Memory content cannot be empty." });
                }

                if (!Enum.TryParse<MemoryCategory>(request.Category, true, out var category))
                {
                    return Results.BadRequest(
                        new { error = $"Invalid memory category '{request.Category}'." }
                    );
                }

                var character = await characterRepository.GetByIdAsync(
                    request.CharacterId,
                    cancellationToken
                );
                if (character is null)
                {
                    return Results.BadRequest(
                        new { error = $"Character '{request.CharacterId}' was not found." }
                    );
                }

                if (request.ConversationId.HasValue)
                {
                    var conversation = await conversationRepository.GetByIdWithMessagesAsync(
                        request.ConversationId.Value,
                        cancellationToken
                    );

                    if (conversation is null)
                    {
                        return Results.BadRequest(
                            new
                            {
                                error = $"Conversation '{request.ConversationId.Value}' was not found.",
                            }
                        );
                    }

                    if (conversation.CharacterId != request.CharacterId)
                    {
                        return Results.BadRequest(
                            new
                            {
                                error = "Conversation does not belong to the specified character.",
                            }
                        );
                    }
                }

                var memoryItem = new MemoryItem
                {
                    Id = Guid.NewGuid(),
                    CharacterId = request.CharacterId,
                    ConversationId = request.ConversationId,
                    ScopeType = request.ConversationId.HasValue ? MemoryScopeType.Conversation : MemoryScopeType.Character,
                    Category = category,
                    Kind = category == MemoryCategory.SceneState ? MemoryKind.SceneState : MemoryKind.DurableFact,
                    Content = request.Content.Trim(),
                    IsPinned = request.IsPinned,
                    ReviewStatus = MemoryReviewStatus.Accepted,
                    ConfidenceScore = null,
                    ProposalReason = null,
                    SourceExcerpt = null,
                    NormalizedKey = null,
                    SlotKey = null,
                    SlotFamily = MemorySlotFamily.None,
                    ConflictsWithMemoryItemId = null,
                    ExpiresAt = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await memoryRepository.AddAsync(memoryItem, cancellationToken);
                await memoryRepository.SaveChangesAsync(cancellationToken);
                await retrievalService.IndexMemoryAsync(memoryItem, cancellationToken);

                return Results.Ok(ToResponse(memoryItem));
            }
        );

        group.MapPost("/proposals/generate/{conversationId:guid}", async (
            Guid conversationId,
            IMemoryProposalService proposalService,
            CancellationToken cancellationToken) =>
        {
            var result = await proposalService.GenerateForConversationAsync(conversationId, cancellationToken);

            return Results.Ok(new GenerateMemoryProposalsResponse
            {
                AttemptedCandidates = result.AttemptedCandidates,
                CreatedProposalCount = result.CreatedProposalCount,
                AutoSavedSceneStateCount = result.AutoSavedSceneStateCount,
                AutoAcceptedDurableCount = result.AutoAcceptedDurableCount,
                SceneStateReplacedCount = result.SceneStateReplacedCount,
                MergedDurableProposalCount = result.MergedDurableProposalCount,
                ConflictingDurableProposalCount = result.ConflictingDurableProposalCount,
                SkippedLowConfidenceCount = result.SkippedLowConfidenceCount,
                SkippedDuplicateCount = result.SkippedDuplicateCount,
                ConflictAnnotatedCount = result.ConflictAnnotatedCount,
                InvalidCandidateCount = result.InvalidCandidateCount
            });
        });

        group.MapGet("/conflicts/by-conversation/{conversationId:guid}", async (
            Guid conversationId,
            IMemoryRepository memoryRepository,
            CancellationToken cancellationToken) =>
        {
            var memories = await memoryRepository.ListByConversationAsync(conversationId, cancellationToken);

            var conflictingProposals = memories
                .Where(x => x.ReviewStatus == MemoryReviewStatus.Proposed &&
                            x.ConflictsWithMemoryItemId != null)
                .OrderByDescending(x => x.UpdatedAt)
                .ToList();

            var responses = new List<MemoryConflictResponse>();

            foreach (var proposal in conflictingProposals)
            {
                var conflictingMemory = await memoryRepository.GetByIdAsync(
                    proposal.ConflictsWithMemoryItemId!.Value,
                    cancellationToken);

                if (conflictingMemory is null)
                {
                    continue;
                }

                responses.Add(new MemoryConflictResponse
                {
                    ProposalMemoryId = proposal.Id,
                    ProposalCategory = proposal.Category.ToString(),
                    ProposalKind = proposal.Kind.ToString(),
                    ProposalContent = proposal.Content,
                    ProposalSlotKey = proposal.SlotKey,
                    ProposalConfidenceScore = proposal.ConfidenceScore,
                    ProposalReason = proposal.ProposalReason,
                    ProposalSourceExcerpt = proposal.SourceExcerpt,
                    ConflictingMemoryId = conflictingMemory.Id,
                    ConflictingMemoryCategory = conflictingMemory.Category.ToString(),
                    ConflictingMemoryKind = conflictingMemory.Kind.ToString(),
                    ConflictingMemoryContent = conflictingMemory.Content,
                    ConflictingMemorySlotKey = conflictingMemory.SlotKey,
                    ConflictingMemoryReviewStatus = conflictingMemory.ReviewStatus.ToString(),
                    ProposalUpdatedAt = proposal.UpdatedAt,
                    ConflictingMemoryUpdatedAt = conflictingMemory.UpdatedAt
                });
            }

            return Results.Ok(responses);
        });

        group.MapGet("/conflicts/by-character/{characterId:guid}", async (
            Guid characterId,
            IMemoryRepository memoryRepository,
            CancellationToken cancellationToken) =>
        {
            var memories = await memoryRepository.ListByCharacterAsync(characterId, cancellationToken);

            var conflictingProposals = memories
                .Where(x => x.ReviewStatus == MemoryReviewStatus.Proposed &&
                            x.ConflictsWithMemoryItemId != null)
                .OrderByDescending(x => x.UpdatedAt)
                .ToList();

            var memoryById = memories.ToDictionary(x => x.Id, x => x);

            var responses = conflictingProposals
                .Where(x => x.ConflictsWithMemoryItemId.HasValue && memoryById.ContainsKey(x.ConflictsWithMemoryItemId.Value))
                .Select(proposal =>
                {
                    var conflictingMemory = memoryById[proposal.ConflictsWithMemoryItemId!.Value];

                    return new MemoryConflictResponse
                    {
                        ProposalMemoryId = proposal.Id,
                        ProposalCategory = proposal.Category.ToString(),
                        ProposalKind = proposal.Kind.ToString(),
                        ProposalContent = proposal.Content,
                        ProposalSlotKey = proposal.SlotKey,
                        ProposalConfidenceScore = proposal.ConfidenceScore,
                        ProposalReason = proposal.ProposalReason,
                        ProposalSourceExcerpt = proposal.SourceExcerpt,
                        ConflictingMemoryId = conflictingMemory.Id,
                        ConflictingMemoryCategory = conflictingMemory.Category.ToString(),
                        ConflictingMemoryKind = conflictingMemory.Kind.ToString(),
                        ConflictingMemoryContent = conflictingMemory.Content,
                        ConflictingMemorySlotKey = conflictingMemory.SlotKey,
                        ConflictingMemoryReviewStatus = conflictingMemory.ReviewStatus.ToString(),
                        ProposalUpdatedAt = proposal.UpdatedAt,
                        ConflictingMemoryUpdatedAt = conflictingMemory.UpdatedAt
                    };
                })
                .ToList();

            return Results.Ok(responses);
        });

        group.MapPost("/conflicts/{proposalMemoryId:guid}/accept", async (
            Guid proposalMemoryId,
            IMemoryRepository memoryRepository,
            IConversationRetrievalSyncService retrievalSyncService,
            CancellationToken cancellationToken) =>
        {
            var proposal = await memoryRepository.GetByIdAsync(proposalMemoryId, cancellationToken);
            if (proposal is null)
            {
                return Results.NotFound();
            }

            if (proposal.ReviewStatus != MemoryReviewStatus.Proposed)
            {
                return Results.BadRequest(new MemoryConflictResolutionResponse
                {
                    ProposalMemoryId = proposalMemoryId,
                    Action = "Accept",
                    Succeeded = false,
                    Message = "Only proposed memories can be accepted from the conflict panel.",
                    ConflictingMemoryId = proposal.ConflictsWithMemoryItemId,
                    ConflictingMemoryRejected = false,
                    RetrievalReindexed = false
                });
            }

            var conflictingRejected = false;
            MemoryItem? conflictingMemory = null;

            if (proposal.ConflictsWithMemoryItemId.HasValue)
            {
                conflictingMemory = await memoryRepository.GetByIdAsync(
                    proposal.ConflictsWithMemoryItemId.Value,
                    cancellationToken);

                if (conflictingMemory is not null &&
                    conflictingMemory.ReviewStatus == MemoryReviewStatus.Accepted &&
                    conflictingMemory.Id != proposal.Id &&
                    string.Equals(conflictingMemory.SlotKey, proposal.SlotKey, StringComparison.Ordinal) &&
                    conflictingMemory.Kind == proposal.Kind)
                {
                    conflictingMemory.ReviewStatus = MemoryReviewStatus.Rejected;
                    conflictingMemory.UpdatedAt = DateTime.UtcNow;
                    conflictingRejected = true;
                }
            }

            proposal.ReviewStatus = MemoryReviewStatus.Accepted;
            proposal.ConflictsWithMemoryItemId = null;
            proposal.UpdatedAt = DateTime.UtcNow;

            await memoryRepository.SaveChangesAsync(cancellationToken);

            var reindexed = false;
            if (proposal.ConversationId.HasValue)
            {
                await retrievalSyncService.ReindexConversationAsync(
                    proposal.ConversationId.Value,
                    cancellationToken);

                reindexed = true;
            }

            return Results.Ok(new MemoryConflictResolutionResponse
            {
                ProposalMemoryId = proposalMemoryId,
                Action = "Accept",
                Succeeded = true,
                Message = conflictingRejected
                    ? "Proposal accepted and conflicting accepted memory was rejected."
                    : "Proposal accepted.",
                ConflictingMemoryId = conflictingMemory?.Id,
                ConflictingMemoryRejected = conflictingRejected,
                RetrievalReindexed = reindexed
            });
        });

        group.MapPost("/conflicts/{proposalMemoryId:guid}/reject", async (
            Guid proposalMemoryId,
            IMemoryRepository memoryRepository,
            IConversationRetrievalSyncService retrievalSyncService,
            CancellationToken cancellationToken) =>
        {
            var proposal = await memoryRepository.GetByIdAsync(proposalMemoryId, cancellationToken);
            if (proposal is null)
            {
                return Results.NotFound();
            }

            if (proposal.ReviewStatus != MemoryReviewStatus.Proposed)
            {
                return Results.BadRequest(new MemoryConflictResolutionResponse
                {
                    ProposalMemoryId = proposalMemoryId,
                    Action = "Reject",
                    Succeeded = false,
                    Message = "Only proposed memories can be rejected from the conflict panel.",
                    ConflictingMemoryId = proposal.ConflictsWithMemoryItemId,
                    ConflictingMemoryRejected = false,
                    RetrievalReindexed = false
                });
            }

            proposal.ReviewStatus = MemoryReviewStatus.Rejected;
            proposal.UpdatedAt = DateTime.UtcNow;

            await memoryRepository.SaveChangesAsync(cancellationToken);

            var reindexed = false;
            if (proposal.ConversationId.HasValue)
            {
                await retrievalSyncService.ReindexConversationAsync(
                    proposal.ConversationId.Value,
                    cancellationToken);

                reindexed = true;
            }

            return Results.Ok(new MemoryConflictResolutionResponse
            {
                ProposalMemoryId = proposalMemoryId,
                Action = "Reject",
                Succeeded = true,
                Message = "Proposal rejected.",
                ConflictingMemoryId = proposal.ConflictsWithMemoryItemId,
                ConflictingMemoryRejected = false,
                RetrievalReindexed = reindexed
            });
        });

        group.MapPut("/{memoryId:guid}", async (
            Guid memoryId,
            UpdateMemoryItemRequest request,
            IMemoryRepository memoryRepository,
            IConversationRetrievalSyncService retrievalSyncService,
            CancellationToken cancellationToken) =>
        {
            var memory = await memoryRepository.GetByIdAsync(memoryId, cancellationToken);
            if (memory is null)
            {
                return Results.NotFound();
            }

            memory.Content = request.Content.Trim();

            if (!string.IsNullOrWhiteSpace(request.Category) &&
                Enum.TryParse<MemoryCategory>(request.Category, true, out var category))
            {
                memory.Category = category;
            }

            if (!string.IsNullOrWhiteSpace(request.Kind) &&
                Enum.TryParse<MemoryKind>(request.Kind, true, out var kind))
            {
                memory.Kind = kind;
            }

            if (!string.IsNullOrWhiteSpace(request.ReviewStatus) &&
                Enum.TryParse<MemoryReviewStatus>(request.ReviewStatus, true, out var reviewStatus))
            {
                memory.ReviewStatus = reviewStatus;
            }
            if (!string.IsNullOrWhiteSpace(request.SlotFamily) &&
                Enum.TryParse<MemorySlotFamily>(request.SlotFamily, true, out var slotFamily))
            {
                memory.SlotFamily = slotFamily;
            }

            if (request.IsPinned.HasValue)
            {
                memory.IsPinned = request.IsPinned.Value;
            }

            memory.ProposalReason = request.ProposalReason;
            memory.SourceExcerpt = request.SourceExcerpt;
            if (request.ExpiresAt.HasValue)
            {
                memory.ExpiresAt = request.ExpiresAt;
            }
            else
            {
                memory.ExpiresAt = null;
            }

            memory.SlotKey = NormalizeSlotKey(request.SlotKey);
            memory.NormalizedKey = BuildNormalizedKey(memory.Category, memory.Content);
            memory.UpdatedAt = DateTime.UtcNow;

            await memoryRepository.SaveChangesAsync(cancellationToken);

            if (memory.ConversationId.HasValue)
            {
                await retrievalSyncService.ReindexConversationAsync(memory.ConversationId.Value, cancellationToken);
            }

            return Results.Ok(ToResponse(memory));
        });

        group.MapPost("/{sourceMemoryId:guid}/merge-into/{targetMemoryId:guid}", async (
            Guid sourceMemoryId,
            Guid targetMemoryId,
            MergeMemoryItemsRequest request,
            IMemoryRepository memoryRepository,
            IConversationRetrievalSyncService retrievalSyncService,
            CancellationToken cancellationToken) =>
        {
            if (sourceMemoryId == targetMemoryId)
            {
                return Results.BadRequest(new MergeMemoryItemsResponse
                {
                    SourceMemoryId = sourceMemoryId,
                    TargetMemoryId = targetMemoryId,
                    Succeeded = false,
                    Message = "Source and target memory must be different.",
                    TargetSlotKey = null,
                    TargetReviewStatus = null,
                    SourceRejected = false,
                    RetrievalReindexed = false
                });
            }

            var source = await memoryRepository.GetByIdAsync(sourceMemoryId, cancellationToken);
            var target = await memoryRepository.GetByIdAsync(targetMemoryId, cancellationToken);

            if (source is null || target is null)
            {
                return Results.NotFound();
            }

            if ((source.Kind == MemoryKind.SceneState ||
                 target.Kind == MemoryKind.SceneState) &&
                source.Kind != target.Kind)
            {
                return Results.BadRequest(new MergeMemoryItemsResponse
                {
                    SourceMemoryId = sourceMemoryId,
                    TargetMemoryId = targetMemoryId,
                    Succeeded = false,
                    Message = "Scene-state and durable memory cannot be merged together.",
                    TargetSlotKey = target.SlotKey,
                    TargetReviewStatus = target.ReviewStatus.ToString(),
                    SourceRejected = false,
                    RetrievalReindexed = false
                });
            }

            var effectiveSourceFamily = source.SlotFamily;
            var effectiveTargetFamily = target.SlotFamily;

            MemorySlotFamily? overrideFamily = null;
            if (!string.IsNullOrWhiteSpace(request.SlotFamilyOverride) &&
                Enum.TryParse<MemorySlotFamily>(request.SlotFamilyOverride, true, out var parsedOverrideFamily))
            {
                overrideFamily = parsedOverrideFamily;
            }

            if (source.Kind == MemoryKind.SceneState &&
                target.Kind == MemoryKind.SceneState)
            {
                var sourceConcrete = effectiveSourceFamily != MemorySlotFamily.None &&
                                     effectiveSourceFamily != MemorySlotFamily.Misc;

                var targetConcrete = effectiveTargetFamily != MemorySlotFamily.None &&
                                     effectiveTargetFamily != MemorySlotFamily.Misc;

                if (overrideFamily is null &&
                    sourceConcrete &&
                    targetConcrete &&
                    effectiveSourceFamily != effectiveTargetFamily)
                {
                    return Results.BadRequest(new MergeMemoryItemsResponse
                    {
                        SourceMemoryId = sourceMemoryId,
                        TargetMemoryId = targetMemoryId,
                        Succeeded = false,
                        Message = $"Scene-state merge blocked because families differ ({effectiveSourceFamily} vs {effectiveTargetFamily}). Provide a SlotFamilyOverride to force it.",
                        TargetSlotKey = target.SlotKey,
                        TargetReviewStatus = target.ReviewStatus.ToString(),
                        SourceRejected = false,
                        RetrievalReindexed = false
                    });
                }
            }

            if (request.PreferSourceContent)
            {
                target.Content = source.Content;
                target.Category = source.Category;
                target.Kind = source.Kind;
                target.ProposalReason = MergeText(target.ProposalReason, source.ProposalReason);
                target.SourceExcerpt = MergeText(target.SourceExcerpt, source.SourceExcerpt);
            }

            if (overrideFamily.HasValue)
            {
                target.SlotFamily = overrideFamily.Value;
            }
            else if (request.PreferSourceContent)
            {
                target.SlotFamily = source.SlotFamily;
            }

            target.SlotKey = NormalizeSlotKey(request.SlotKeyOverride) ??
                             target.SlotKey ??
                             source.SlotKey;

            target.NormalizedKey = BuildNormalizedKey(target.Category, target.Content);

            if (!request.PreserveTargetReviewStatus)
            {
                target.ReviewStatus = source.ReviewStatus;
            }

            if (target.Kind == MemoryKind.SceneState)
            {
                target.ExpiresAt = null;
            }
            else
            {
                target.ExpiresAt = null;
            }

            target.UpdatedAt = DateTime.UtcNow;

            var sourceRejected = false;
            if (request.RejectSourceAfterMerge)
            {
                source.ReviewStatus = MemoryReviewStatus.Rejected;
                source.UpdatedAt = DateTime.UtcNow;
                sourceRejected = true;
            }

            await memoryRepository.SaveChangesAsync(cancellationToken);

            var reindexed = false;
            var conversationId = target.ConversationId ?? source.ConversationId;
            if (conversationId.HasValue)
            {
                await retrievalSyncService.ReindexConversationAsync(conversationId.Value, cancellationToken);
                reindexed = true;
            }

            return Results.Ok(new MergeMemoryItemsResponse
            {
                SourceMemoryId = sourceMemoryId,
                TargetMemoryId = targetMemoryId,
                Succeeded = true,
                Message = "Memory merge completed.",
                TargetSlotKey = target.SlotKey,
                TargetReviewStatus = target.ReviewStatus.ToString(),
                SourceRejected = sourceRejected,
                RetrievalReindexed = reindexed
            });
        });

        group.MapPut(
            "/{id:guid}/review",
            async (
                Guid id,
                ReviewMemoryRequest request,
                IMemoryRepository memoryRepository,
                IRetrievalService retrievalService,
                CancellationToken cancellationToken
            ) =>
            {
                var memoryItem = await memoryRepository.GetByIdAsync(id, cancellationToken);
                if (memoryItem is null)
                {
                    return Results.NotFound();
                }

                if (!Enum.TryParse<MemoryReviewStatus>(request.Status, true, out var reviewStatus))
                {
                    return Results.BadRequest(
                        new { error = $"Invalid review status '{request.Status}'." }
                    );
                }

                memoryItem.ReviewStatus = reviewStatus;

                if (request.IsPinned.HasValue)
                {
                    memoryItem.IsPinned = request.IsPinned.Value;
                }

                memoryItem.UpdatedAt = DateTime.UtcNow;

                await memoryRepository.SaveChangesAsync(cancellationToken);

                if (memoryItem.ReviewStatus == MemoryReviewStatus.Accepted)
                {
                    await retrievalService.IndexMemoryAsync(memoryItem, cancellationToken);
                }
                else
                {
                    await retrievalService.RemoveSourceAsync(
                        "Memory",
                        memoryItem.Id,
                        cancellationToken
                    );
                }

                return Results.Ok(ToResponse(memoryItem));
            }
        );

        group.MapPost("/{memoryId:guid}/promote-to-character", async (
            Guid memoryId,
            IMemoryRepository memoryRepository,
            IMemoryOperationAuditService memoryOperationAuditService,
            CancellationToken cancellationToken) =>
        {
            var memoryItem = await memoryRepository.GetByIdAsync(memoryId, cancellationToken);
            if (memoryItem is null)
            {
                return Results.NotFound();
            }

            if (memoryItem.ScopeType == MemoryScopeType.Character)
            {
                return Results.Ok();
            }

            var beforeSnapshot = MemoryAuditSnapshot.From(memoryItem);

            memoryItem.ScopeType = MemoryScopeType.Character;
            memoryItem.ConversationId = null;
            memoryItem.ExpiresAt = null;
            memoryItem.UpdatedAt = DateTime.UtcNow;

            await memoryRepository.SaveChangesAsync(cancellationToken);

            await memoryOperationAuditService.RecordAsync(
                memoryItem.Id,
                MemoryOperationType.PromotedToCharacter,
                beforeState: beforeSnapshot,
                afterState: MemoryAuditSnapshot.From(memoryItem),
                conversationId: beforeSnapshot.ConversationId,
                characterId: memoryItem.CharacterId,
                note: "Promoted from conversation scope to character scope",
                cancellationToken: cancellationToken);

            return Results.Ok();
        });

        return app;
    }

    private static MemoryItemResponse ToResponse(MemoryItem x) =>
        new()
        {
            Id = x.Id,
            Category = x.Category.ToString(),
            Kind = x.Kind.ToString(),
            ScopeType = x.ScopeType.ToString(),
            Content = x.Content,
            IsPinned = x.IsPinned,
            ReviewStatus = x.ReviewStatus.ToString(),
            ConfidenceScore = x.ConfidenceScore,
            ProposalReason = x.ProposalReason,
            SourceExcerpt = x.SourceExcerpt,
            NormalizedKey = x.NormalizedKey,
            SlotKey = x.SlotKey,
            SlotFamily = x.SlotFamily.ToString(),
            ConflictsWithMemoryItemId = x.ConflictsWithMemoryItemId,
            SourceMessageSequenceNumber = x.SourceMessageSequenceNumber,
            LastObservedSequenceNumber = x.LastObservedSequenceNumber,
            SupersededAtSequenceNumber = x.SupersededAtSequenceNumber,
            ExpiresAt = x.ExpiresAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    private static string BuildNormalizedKey(MemoryCategory category, string content)
    {
        var chars = content
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ')
            .ToArray();

        var normalized = string.Join(
            ' ',
            new string(chars)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return $"{category}:{normalized}";
    }

    private static string? NormalizeSlotKey(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            return null;
        }

        var chars = slotKey
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) || ch == '.' ? ch : '.')
            .ToArray();

        var normalized = new string(chars);

        while (normalized.Contains("..", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("..", ".", StringComparison.Ordinal);
        }

        return normalized.Trim('.');
    }

    private static string? MergeText(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left))
        {
            return string.IsNullOrWhiteSpace(right) ? null : right.Trim();
        }

        if (string.IsNullOrWhiteSpace(right))
        {
            return left.Trim();
        }

        if (string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return left.Trim();
        }

        return $"{left.Trim()} | {right.Trim()}";
    }
}
