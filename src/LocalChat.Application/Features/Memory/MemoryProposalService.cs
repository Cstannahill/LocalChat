using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Memory;

public sealed class MemoryProposalService : IMemoryProposalService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IInferenceProvider _inferenceProvider;
    private readonly MemoryProposalQualityEvaluator _qualityEvaluator;
    private readonly MemoryExtractionClassifier _classifier;
    private readonly MemoryProposalOptions _options;
    private readonly IMemoryPolicyService _memoryPolicyService;
    private readonly IMemoryOperationAuditService _memoryOperationAuditService;
    private readonly ISceneStateExtractionEventRepository _sceneStateEventRepository;
    private readonly IMemoryExtractionAuditEventRepository _auditRepository;

    public MemoryProposalService(
        IConversationRepository conversationRepository,
        IMemoryRepository memoryRepository,
        IInferenceProvider inferenceProvider,
        MemoryProposalQualityEvaluator qualityEvaluator,
        MemoryExtractionClassifier classifier,
        MemoryProposalOptions options,
        IMemoryPolicyService memoryPolicyService,
        IMemoryOperationAuditService memoryOperationAuditService,
        ISceneStateExtractionEventRepository sceneStateEventRepository,
        IMemoryExtractionAuditEventRepository auditRepository)
    {
        _conversationRepository = conversationRepository;
        _memoryRepository = memoryRepository;
        _inferenceProvider = inferenceProvider;
        _qualityEvaluator = qualityEvaluator;
        _classifier = classifier;
        _options = options;
        _memoryPolicyService = memoryPolicyService;
        _memoryOperationAuditService = memoryOperationAuditService;
        _sceneStateEventRepository = sceneStateEventRepository;
        _auditRepository = auditRepository;
    }

    public async Task<MemoryProposalGenerationResult> GenerateForConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages.OrderBy(x => x.SequenceNumber).TakeLast(_options.MaxRecentMessagesForExtraction).ToList();
        var sourceMessageSequenceNumber = orderedMessages.Count == 0 ? 0 : orderedMessages[^1].SequenceNumber;
        if (orderedMessages.Count == 0)
        {
            throw new InvalidOperationException("Cannot generate memory proposals for an empty conversation.");
        }

        var raw = await _inferenceProvider.StreamCompletionAsync(BuildPrompt(conversation, orderedMessages), static (_, _) => Task.CompletedTask, null, cancellationToken);
        var parsed = ParseCandidates(raw).Take(_options.MaxCandidatesPerRun).ToList();
        var existing = (await _memoryRepository.ListForProposalComparisonAsync(conversation.CharacterId, conversation.Id, cancellationToken)).ToList();

        var attempted = 0;
        var createdProposalCount = 0;
        var autoSavedSceneStateCount = 0;
        var autoAcceptedDurableCount = 0;
        var sceneStateReplacedCount = 0;
        var mergedDurableProposalCount = 0;
        var conflictingDurableProposalCount = 0;
        var skippedLowConfidenceCount = 0;
        var skippedDuplicateCount = 0;
        var conflictAnnotatedCount = 0;
        var invalidCandidateCount = 0;

        foreach (var candidate in parsed)
        {
            attempted++;

            var normalized = NormalizeCandidate(candidate);
            var candidateKind = normalized.Category == MemoryCategory.SceneState ? MemoryKind.SceneState : MemoryKind.DurableFact;

            if (string.IsNullOrWhiteSpace(normalized.Content) || string.IsNullOrWhiteSpace(normalized.NormalizedKey) || normalized.NormalizedKey.Length < _options.MinNormalizedKeyLength)
            {
                invalidCandidateCount++;
                await LogAuditAsync(conversation, normalized, candidateKind, "InvalidCandidate", null, "Candidate content/normalized key was invalid.", cancellationToken);
                continue;
            }

            if (normalized.ConfidenceScore < _options.MinConfidenceScore)
            {
                skippedLowConfidenceCount++;
                await LogAuditAsync(conversation, normalized, candidateKind, "SkippedLowConfidence", null, "Candidate was below minimum confidence threshold.", cancellationToken);
                if (candidateKind == MemoryKind.SceneState)
                {
                    await LogSceneStateEventAsync(conversation, normalized, "SkippedLowConfidence", null, "Candidate was below minimum confidence threshold.", cancellationToken);
                }
                continue;
            }

            if (_qualityEvaluator.IsNearDuplicate(normalized.NormalizedKey, existing))
            {
                skippedDuplicateCount++;
                await LogAuditAsync(conversation, normalized, candidateKind, "SkippedDuplicate", null, "Candidate matched an existing normalized key.", cancellationToken);
                if (candidateKind == MemoryKind.SceneState)
                {
                    await LogSceneStateEventAsync(conversation, normalized, "SkippedDuplicate", null, "Candidate matched an existing normalized key.", cancellationToken);
                }
                continue;
            }

            var conflict = _qualityEvaluator.FindLikelyConflict(normalized.Category, normalized.NormalizedKey, normalized.SlotKey ?? string.Empty, existing);
            if (conflict is not null)
            {
                conflictAnnotatedCount++;
            }

            var decision = _classifier.Classify(normalized, conflict is not null);

            if (decision.Kind == MemoryKind.SceneState && decision.ReviewStatus == MemoryReviewStatus.Accepted)
            {
                MemoryItem? tracked = null;
                if (_options.EnforceSingleSceneStatePerFamily && normalized.SlotFamily != MemorySlotFamily.None && normalized.SlotFamily != MemorySlotFamily.Misc)
                {
                    tracked = await _memoryRepository.FindTrackedByFamilyAsync(conversation.CharacterId, conversation.Id, normalized.SlotFamily, MemoryKind.SceneState, cancellationToken);
                }
                tracked ??= await _memoryRepository.FindTrackedBySlotAsync(conversation.CharacterId, conversation.Id, normalized.SlotKey!, MemoryKind.SceneState, cancellationToken);

                if (tracked is not null)
                {
                    var supersededBefore = MemoryAuditSnapshot.From(tracked);
                    var action = string.Equals(tracked.SlotKey, normalized.SlotKey, StringComparison.Ordinal) ? "ReplacedBySlot" : "ReplacedByFamily";
                    if (!string.Equals(tracked.NormalizedKey, normalized.NormalizedKey, StringComparison.Ordinal))
                    {
                        sceneStateReplacedCount++;
                    }

                    await LogSceneStateEventAsync(conversation, normalized, action, tracked, action == "ReplacedByFamily"
                        ? "Family collision: candidate replaced an older scene-state item in the same family."
                        : "Slot replacement: candidate replaced older scene-state item in the same slot.", cancellationToken);

                    await LogAuditAsync(conversation, normalized, MemoryKind.SceneState, action, tracked,
                        action == "ReplacedByFamily" ? "Scene-state candidate replaced older item by family precedence." : "Scene-state candidate replaced older item in the same slot.", cancellationToken);

                    _memoryPolicyService.MarkSuperseded(tracked, sourceMessageSequenceNumber);
                    tracked.UpdatedAt = DateTime.UtcNow;

                    var replacement = new MemoryItem
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = conversation.CharacterId,
                        ConversationId = conversation.Id,
                        ScopeType = _memoryPolicyService.ResolveAutomaticScope(),
                        Category = MemoryCategory.SceneState,
                        Kind = MemoryKind.SceneState,
                        Content = normalized.Content,
                        ReviewStatus = MemoryReviewStatus.Accepted,
                        IsPinned = false,
                        ConfidenceScore = normalized.ConfidenceScore,
                        ProposalReason = normalized.ProposalReason,
                        SourceExcerpt = normalized.SourceExcerpt,
                        NormalizedKey = normalized.NormalizedKey,
                        SlotKey = normalized.SlotKey,
                        SlotFamily = normalized.SlotFamily,
                        ConflictsWithMemoryItemId = conflict?.Id,
                        ExpiresAt = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _memoryPolicyService.ApplyAutomaticDefaults(
                        replacement,
                        conversation.Id,
                        conversation.CharacterId,
                        sourceMessageSequenceNumber);

                    await _memoryRepository.AddAsync(replacement, cancellationToken);
                    existing.Add(replacement);

                    await _memoryOperationAuditService.RecordAsync(
                        replacement.Id,
                        MemoryOperationType.CreatedAutomatic,
                        beforeState: null,
                        afterState: MemoryAuditSnapshot.From(replacement),
                        conversationId: replacement.ConversationId,
                        characterId: replacement.CharacterId,
                        messageSequenceNumber: sourceMessageSequenceNumber,
                        note: "Automatic memory creation",
                        cancellationToken: cancellationToken);

                    await _memoryOperationAuditService.RecordAsync(
                        tracked.Id,
                        MemoryOperationType.SupersededAutomatic,
                        beforeState: supersededBefore,
                        afterState: MemoryAuditSnapshot.From(tracked),
                        conversationId: tracked.ConversationId,
                        characterId: tracked.CharacterId,
                        messageSequenceNumber: sourceMessageSequenceNumber,
                        note: "Automatic supersede",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await LogSceneStateEventAsync(conversation, normalized, "Inserted", null, "New scene-state item inserted.", cancellationToken);
                    await LogAuditAsync(conversation, normalized, MemoryKind.SceneState, "InsertedSceneState", null, "New scene-state item inserted.", cancellationToken);

                    var item = new MemoryItem
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = conversation.CharacterId,
                        ConversationId = conversation.Id,
                        ScopeType = _memoryPolicyService.ResolveAutomaticScope(),
                        Category = MemoryCategory.SceneState,
                        Kind = MemoryKind.SceneState,
                        Content = normalized.Content,
                        ReviewStatus = MemoryReviewStatus.Accepted,
                        IsPinned = false,
                        ConfidenceScore = normalized.ConfidenceScore,
                        ProposalReason = normalized.ProposalReason,
                        SourceExcerpt = normalized.SourceExcerpt,
                        NormalizedKey = normalized.NormalizedKey,
                        SlotKey = normalized.SlotKey,
                        SlotFamily = normalized.SlotFamily,
                        ConflictsWithMemoryItemId = conflict?.Id,
                        ExpiresAt = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _memoryPolicyService.ApplyAutomaticDefaults(
                        item,
                        conversation.Id,
                        conversation.CharacterId,
                        sourceMessageSequenceNumber);

                    await _memoryRepository.AddAsync(item, cancellationToken);
                    existing.Add(item);

                    await _memoryOperationAuditService.RecordAsync(
                        item.Id,
                        MemoryOperationType.CreatedAutomatic,
                        beforeState: null,
                        afterState: MemoryAuditSnapshot.From(item),
                        conversationId: item.ConversationId,
                        characterId: item.CharacterId,
                        messageSequenceNumber: sourceMessageSequenceNumber,
                        note: "Automatic memory creation",
                        cancellationToken: cancellationToken);
                }

                autoSavedSceneStateCount++;
                continue;
            }

            var trackedDurable = await _memoryRepository.FindTrackedBySlotAsync(conversation.CharacterId, conversation.Id, normalized.SlotKey!, MemoryKind.DurableFact, cancellationToken);
            if (trackedDurable is not null)
            {
                if (string.Equals(trackedDurable.NormalizedKey, normalized.NormalizedKey, StringComparison.Ordinal))
                {
                    skippedDuplicateCount++;
                    await LogAuditAsync(conversation, normalized, MemoryKind.DurableFact, "SkippedDuplicate", trackedDurable, "Durable candidate matched existing slot and normalized key.", cancellationToken);
                    continue;
                }

                if (_qualityEvaluator.ShouldMergeIntoExistingProposal(trackedDurable, normalized))
                {
                    var beforeSnapshot = MemoryAuditSnapshot.From(trackedDurable);
                    trackedDurable.Content = normalized.Content;
                    trackedDurable.Category = decision.Category;
                    trackedDurable.Kind = MemoryKind.DurableFact;
                    trackedDurable.ReviewStatus = MemoryReviewStatus.Proposed;
                    trackedDurable.ConfidenceScore = normalized.ConfidenceScore;
                    trackedDurable.ProposalReason = AppendReason(normalized.ProposalReason, "Merged into existing durable-memory proposal for the same slot.");
                    trackedDurable.SourceExcerpt = normalized.SourceExcerpt;
                    trackedDurable.NormalizedKey = normalized.NormalizedKey;
                    trackedDurable.SlotKey = normalized.SlotKey;
                    trackedDurable.SlotFamily = normalized.SlotFamily;
                    trackedDurable.ConflictsWithMemoryItemId = null;
                    trackedDurable.ExpiresAt = null;
                    _memoryPolicyService.ReinforceObservation(trackedDurable, sourceMessageSequenceNumber);
                    trackedDurable.UpdatedAt = DateTime.UtcNow;

                    await LogAuditAsync(conversation, normalized, MemoryKind.DurableFact, "MergedDurableProposal", trackedDurable, "Higher-confidence durable proposal merged into existing proposal for same slot.", cancellationToken);

                    await _memoryOperationAuditService.RecordAsync(
                        trackedDurable.Id,
                        MemoryOperationType.ReinforcedAutomatic,
                        beforeState: beforeSnapshot,
                        afterState: MemoryAuditSnapshot.From(trackedDurable),
                        conversationId: trackedDurable.ConversationId,
                        characterId: trackedDurable.CharacterId,
                        messageSequenceNumber: sourceMessageSequenceNumber,
                        note: "Automatic reinforcement",
                        cancellationToken: cancellationToken);

                    mergedDurableProposalCount++;
                    continue;
                }

                if (trackedDurable.ReviewStatus == MemoryReviewStatus.Accepted)
                {
                    var conflictProposal = new MemoryItem
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = conversation.CharacterId,
                        ConversationId = conversation.Id,
                        ScopeType = _memoryPolicyService.ResolveAutomaticScope(),
                        Category = decision.Category,
                        Kind = MemoryKind.DurableFact,
                        Content = normalized.Content,
                        ReviewStatus = MemoryReviewStatus.Proposed,
                        IsPinned = false,
                        ConfidenceScore = normalized.ConfidenceScore,
                        ProposalReason = AppendReason(normalized.ProposalReason, $"Possible contradiction with accepted memory in slot '{normalized.SlotKey}'."),
                        SourceExcerpt = normalized.SourceExcerpt,
                        NormalizedKey = normalized.NormalizedKey,
                        SlotKey = normalized.SlotKey,
                        SlotFamily = normalized.SlotFamily,
                        ConflictsWithMemoryItemId = trackedDurable.Id,
                        ExpiresAt = null,
                        SourceMessageSequenceNumber = sourceMessageSequenceNumber,
                        LastObservedSequenceNumber = sourceMessageSequenceNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _memoryRepository.AddAsync(conflictProposal, cancellationToken);
                    existing.Add(conflictProposal);

                    await _memoryOperationAuditService.RecordAsync(
                        conflictProposal.Id,
                        MemoryOperationType.CreatedAutomatic,
                        beforeState: null,
                        afterState: MemoryAuditSnapshot.From(conflictProposal),
                        conversationId: conflictProposal.ConversationId,
                        characterId: conflictProposal.CharacterId,
                        messageSequenceNumber: sourceMessageSequenceNumber,
                        note: "Automatic memory creation",
                        cancellationToken: cancellationToken);

                    await LogAuditAsync(conversation, normalized, MemoryKind.DurableFact, "ProposedConflict", trackedDurable, "Durable candidate conflicted with accepted memory in the same slot.", cancellationToken);
                    conflictingDurableProposalCount++;
                    createdProposalCount++;
                    continue;
                }
            }

            var memoryItem = new MemoryItem
            {
                Id = Guid.NewGuid(),
                CharacterId = conversation.CharacterId,
                ConversationId = conversation.Id,
                ScopeType = _memoryPolicyService.ResolveAutomaticScope(),
                Category = decision.Category,
                Kind = decision.Kind,
                Content = normalized.Content,
                ReviewStatus = decision.ReviewStatus,
                IsPinned = false,
                ConfidenceScore = normalized.ConfidenceScore,
                ProposalReason = normalized.ProposalReason,
                SourceExcerpt = normalized.SourceExcerpt,
                NormalizedKey = normalized.NormalizedKey,
                SlotKey = normalized.SlotKey,
                SlotFamily = normalized.SlotFamily,
                ConflictsWithMemoryItemId = conflict?.Id,
                ExpiresAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _memoryPolicyService.ApplyAutomaticDefaults(
                memoryItem,
                conversation.Id,
                conversation.CharacterId,
                sourceMessageSequenceNumber);

            await _memoryRepository.AddAsync(memoryItem, cancellationToken);
            existing.Add(memoryItem);

            await _memoryOperationAuditService.RecordAsync(
                memoryItem.Id,
                MemoryOperationType.CreatedAutomatic,
                beforeState: null,
                afterState: MemoryAuditSnapshot.From(memoryItem),
                conversationId: memoryItem.ConversationId,
                characterId: memoryItem.CharacterId,
                messageSequenceNumber: sourceMessageSequenceNumber,
                note: "Automatic memory creation",
                cancellationToken: cancellationToken);

            if (decision.ReviewStatus == MemoryReviewStatus.Accepted)
            {
                await LogAuditAsync(conversation, normalized, MemoryKind.DurableFact, "AcceptedDurable", null, "Durable fact was auto-accepted.", cancellationToken);
                autoAcceptedDurableCount++;
            }
            else
            {
                await LogAuditAsync(conversation, normalized, MemoryKind.DurableFact, "ProposedDurable", null, "Durable candidate was stored as proposed.", cancellationToken);
                createdProposalCount++;
            }
        }

        await _memoryRepository.SaveChangesAsync(cancellationToken);
        await _sceneStateEventRepository.SaveChangesAsync(cancellationToken);
        await _auditRepository.SaveChangesAsync(cancellationToken);

        return new MemoryProposalGenerationResult
        {
            AttemptedCandidates = attempted,
            CreatedProposalCount = createdProposalCount,
            AutoSavedSceneStateCount = autoSavedSceneStateCount,
            AutoAcceptedDurableCount = autoAcceptedDurableCount,
            SceneStateReplacedCount = sceneStateReplacedCount,
            MergedDurableProposalCount = mergedDurableProposalCount,
            ConflictingDurableProposalCount = conflictingDurableProposalCount,
            SkippedLowConfidenceCount = skippedLowConfidenceCount,
            SkippedDuplicateCount = skippedDuplicateCount,
            ConflictAnnotatedCount = conflictAnnotatedCount,
            InvalidCandidateCount = invalidCandidateCount
        };
    }

    private ExtractedMemoryCandidate NormalizeCandidate(ExtractedMemoryCandidate candidate)
    {
        var normalizedKey = _qualityEvaluator.NormalizeKey(candidate.Category, candidate.Content);
        var slotKey = _qualityEvaluator.BuildSlotKey(candidate.Category, candidate.Content, candidate.SlotKey);
        var slotFamily = _qualityEvaluator.BuildSlotFamily(candidate.Category, candidate.Content, candidate.SlotKey, candidate.SlotFamily == MemorySlotFamily.None ? null : candidate.SlotFamily.ToString());

        return new ExtractedMemoryCandidate
        {
            Category = candidate.Category,
            Content = candidate.Content,
            ConfidenceScore = candidate.ConfidenceScore,
            ProposalReason = candidate.ProposalReason,
            SourceExcerpt = candidate.SourceExcerpt,
            ExplicitnessScore = candidate.ExplicitnessScore,
            PersistenceScore = candidate.PersistenceScore,
            SceneBoundScore = candidate.SceneBoundScore,
            ConflictRiskScore = candidate.ConflictRiskScore,
            NormalizedKey = normalizedKey,
            SlotKey = slotKey,
            SlotFamily = slotFamily
        };
    }

    private Task LogSceneStateEventAsync(Domain.Entities.Conversations.Conversation conversation, ExtractedMemoryCandidate candidate, string action, MemoryItem? replaced, string? notes, CancellationToken cancellationToken)
        => _sceneStateEventRepository.AddAsync(new SceneStateExtractionEvent
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            CharacterId = conversation.CharacterId,
            SlotFamily = candidate.SlotFamily,
            SlotKey = candidate.SlotKey,
            CandidateContent = candidate.Content,
            CandidateNormalizedKey = candidate.NormalizedKey,
            ConfidenceScore = candidate.ConfidenceScore,
            Action = action,
            ReplacedMemoryItemId = replaced?.Id,
            ReplacedMemoryContent = replaced?.Content,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

    private Task LogAuditAsync(Domain.Entities.Conversations.Conversation conversation, ExtractedMemoryCandidate candidate, MemoryKind kind, string action, MemoryItem? existingMemory, string? notes, CancellationToken cancellationToken)
        => _auditRepository.AddAsync(new MemoryExtractionAuditEvent
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            CharacterId = conversation.CharacterId,
            Category = candidate.Category,
            Kind = kind,
            SlotFamily = candidate.SlotFamily,
            SlotKey = candidate.SlotKey,
            CandidateContent = candidate.Content,
            CandidateNormalizedKey = candidate.NormalizedKey,
            ConfidenceScore = candidate.ConfidenceScore,
            Action = action,
            ExistingMemoryItemId = existingMemory?.Id,
            ExistingMemoryContent = existingMemory?.Content,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

    private static string BuildPrompt(Domain.Entities.Conversations.Conversation conversation, IReadOnlyList<Domain.Entities.Conversations.Message> orderedMessages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Extract memory candidates from the conversation.");
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Goals:");
        sb.AppendLine("- identify stable durable facts about the user, character, relationship, or world");
        sb.AppendLine("- identify active scene-state facts that matter right now");
        sb.AppendLine("- do not invent facts");
        sb.AppendLine("- prefer explicit facts over weak inference");
        sb.AppendLine("- use SceneState only for temporary current-scene details");
        sb.AppendLine("- provide a slotKey representing the semantic slot");
        sb.AppendLine("- provide a slotFamily using one of: None, Outfit, Location, PoseAction, Possession, EmotionalState, RelationshipState, Preference, Identity, WorldState, Misc");
        sb.AppendLine();
        sb.AppendLine("Return this JSON shape:");
        sb.AppendLine("""
{
  "proposals": [
    {
      "category": "UserFact | CharacterFact | RelationshipFact | WorldFact | SceneState",
      "content": "string",
      "slotKey": "string",
      "slotFamily": "string",
      "confidence": 0.0,
      "reason": "string",
      "evidence": "string",
      "explicitness": 0.0,
      "persistence": 0.0,
      "sceneBound": 0.0,
      "conflictRisk": 0.0
    }
  ]
}
""");
        sb.AppendLine();
        sb.AppendLine("Character:");
        sb.AppendLine($"Name: {conversation.Character?.Name}");
        sb.AppendLine($"Description: {conversation.Character?.Description}");
        sb.AppendLine($"Scenario: {conversation.Character?.Scenario}");
        sb.AppendLine();
        if (conversation.UserPersona is not null)
        {
            sb.AppendLine("User Persona:");
            sb.AppendLine($"Display Name: {conversation.UserPersona.DisplayName}");
            sb.AppendLine($"Description: {conversation.UserPersona.Description}");
            sb.AppendLine($"Traits: {conversation.UserPersona.Traits}");
            sb.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(conversation.SceneContext))
        {
            sb.AppendLine("Scene Context:");
            sb.AppendLine(conversation.SceneContext);
            sb.AppendLine();
        }
        var summary = conversation.SummaryCheckpoints.OrderByDescending(x => x.EndSequenceNumber).FirstOrDefault()?.SummaryText;
        if (!string.IsNullOrWhiteSpace(summary))
        {
            sb.AppendLine("Rolling Summary:");
            sb.AppendLine(summary);
            sb.AppendLine();
        }
        sb.AppendLine("Recent Conversation:");
        foreach (var message in orderedMessages)
        {
            sb.AppendLine($"{message.Role}: {message.Content}");
        }
        sb.AppendLine();
        sb.AppendLine("Return JSON only.");
        return sb.ToString();
    }

    private static IReadOnlyList<ExtractedMemoryCandidate> ParseCandidates(string raw)
    {
        var cleaned = ExtractJsonPayload(StripCodeFences(raw));
        using var doc = JsonDocument.Parse(cleaned);

        if (!doc.RootElement.TryGetProperty("proposals", out var proposals) || proposals.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ExtractedMemoryCandidate>();
        }

        var results = new List<ExtractedMemoryCandidate>();
        foreach (var item in proposals.EnumerateArray())
        {
            var categoryText = item.TryGetProperty("category", out var categoryProp) ? categoryProp.GetString()?.Trim() : null;
            if (!Enum.TryParse<MemoryCategory>(categoryText, true, out var category)) continue;

            var content = item.TryGetProperty("content", out var contentProp) ? contentProp.GetString()?.Trim() : null;
            if (string.IsNullOrWhiteSpace(content)) continue;

            var slotKey = item.TryGetProperty("slotKey", out var slotProp) ? slotProp.GetString()?.Trim() : null;
            var slotFamilyText = item.TryGetProperty("slotFamily", out var slotFamilyProp) ? slotFamilyProp.GetString()?.Trim() : null;
            var slotFamily = MemorySlotFamily.None;
            if (!string.IsNullOrWhiteSpace(slotFamilyText) && Enum.TryParse<MemorySlotFamily>(slotFamilyText, true, out var parsedSlotFamily))
            {
                slotFamily = parsedSlotFamily;
            }

            var confidence = item.TryGetProperty("confidence", out var confidenceProp) && confidenceProp.TryGetDouble(out var confidenceValue) ? confidenceValue : 0.0;
            var reason = item.TryGetProperty("reason", out var reasonProp) ? reasonProp.GetString()?.Trim() : null;
            var evidence = item.TryGetProperty("evidence", out var evidenceProp) ? evidenceProp.GetString()?.Trim() : null;
            var explicitness = item.TryGetProperty("explicitness", out var explicitnessProp) && explicitnessProp.TryGetDouble(out var explicitnessValue) ? explicitnessValue : confidence;
            var persistence = item.TryGetProperty("persistence", out var persistenceProp) && persistenceProp.TryGetDouble(out var persistenceValue) ? persistenceValue : 0.5;
            var sceneBound = item.TryGetProperty("sceneBound", out var sceneBoundProp) && sceneBoundProp.TryGetDouble(out var sceneBoundValue) ? sceneBoundValue : 0.5;
            var conflictRisk = item.TryGetProperty("conflictRisk", out var conflictRiskProp) && conflictRiskProp.TryGetDouble(out var conflictRiskValue) ? conflictRiskValue : 0.0;

            results.Add(new ExtractedMemoryCandidate
            {
                Category = category,
                Content = content,
                ConfidenceScore = confidence,
                ProposalReason = reason,
                SourceExcerpt = evidence,
                ExplicitnessScore = explicitness,
                PersistenceScore = persistence,
                SceneBoundScore = sceneBound,
                ConflictRiskScore = conflictRisk,
                NormalizedKey = string.Empty,
                SlotKey = slotKey,
                SlotFamily = slotFamily
            });
        }

        return results;
    }

    private static string AppendReason(string? original, string suffix)
        => string.IsNullOrWhiteSpace(original) ? suffix : $"{original} {suffix}";

    private static string StripCodeFences(string raw)
    {
        var trimmed = raw.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;
        var lines = trimmed.Split('\n').ToList();
        if (lines.Count > 0 && lines[0].StartsWith("```", StringComparison.Ordinal)) lines.RemoveAt(0);
        if (lines.Count > 0 && lines[^1].StartsWith("```", StringComparison.Ordinal)) lines.RemoveAt(lines.Count - 1);
        return string.Join('\n', lines).Trim();
    }

    private static string ExtractJsonPayload(string raw)
    {
        var firstBrace = raw.IndexOf('{');
        var lastBrace = raw.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace) return raw[firstBrace..(lastBrace + 1)];
        return raw.Trim();
    }
}
