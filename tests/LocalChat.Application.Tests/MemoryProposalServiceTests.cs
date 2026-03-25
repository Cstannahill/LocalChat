using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Features.Memory;
using LocalChat.Application.Memory;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class MemoryProposalServiceTests
{
    [Fact]
    public async Task GenerateForConversationAsync_ReplacesSceneStateByFamily_WhenFamilyEnforcementEnabled()
    {
        var fixture = CreateFixture();

        fixture.MemoryRepository.Items.Add(new MemoryItem
        {
            Id = Guid.NewGuid(),
            CharacterId = fixture.CharacterId,
            ConversationId = fixture.ConversationId,
            Category = MemoryCategory.SceneState,
            Kind = MemoryKind.SceneState,
            Content = "The character is currently wearing a yellow sundress.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            ConfidenceScore = 0.92,
            NormalizedKey = "SceneState:the character is currently wearing a yellow sundress",
            SlotKey = "scene.character.outfit",
            SlotFamily = MemorySlotFamily.Outfit,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var service = CreateService(
            fixture,
            """
            {
              "proposals": [
                {
                  "category": "SceneState",
                  "content": "The character is currently wearing a black evening gown.",
                  "slotKey": "scene.character.formal-outfit",
                  "slotFamily": "Outfit",
                  "confidence": 0.95,
                  "reason": "Current outfit was explicitly stated.",
                  "evidence": "She smooths the black evening gown over her hips.",
                  "explicitness": 0.96,
                  "persistence": 0.20,
                  "sceneBound": 0.97,
                  "conflictRisk": 0.03
                }
              ]
            }
            """,
            new MemoryProposalOptions
            {
                AutoAcceptDurableFacts = false,
                EnforceSingleSceneStatePerFamily = true
            });

        var result = await service.GenerateForConversationAsync(fixture.ConversationId);

        Assert.Equal(1, result.AutoSavedSceneStateCount);
        Assert.Equal(1, result.SceneStateReplacedCount);

        Assert.Equal(2, fixture.MemoryRepository.Items.Count);
        var superseded = fixture.MemoryRepository.Items.Single(x => x.SupersededAtSequenceNumber.HasValue);
        var active = fixture.MemoryRepository.Items.Single(x => !x.SupersededAtSequenceNumber.HasValue);
        Assert.Equal("The character is currently wearing a black evening gown.", active.Content);
        Assert.Equal(MemorySlotFamily.Outfit, active.SlotFamily);
        Assert.NotNull(superseded.SupersededAtSequenceNumber);
        Assert.Contains(
            fixture.AuditRepository.Items,
            x => x.Action == "ReplacedByFamily" || x.Action == "ReplacedBySlot");
    }

    [Fact]
    public async Task GenerateForConversationAsync_CreatesConflictingDurableProposal_WhenAcceptedFactExistsInSameSlot()
    {
        var fixture = CreateFixture();

        fixture.MemoryRepository.Items.Add(new MemoryItem
        {
            Id = Guid.NewGuid(),
            CharacterId = fixture.CharacterId,
            ConversationId = fixture.ConversationId,
            Category = MemoryCategory.UserFact,
            Kind = MemoryKind.DurableFact,
            Content = "The user prefers TypeScript over JavaScript.",
            ReviewStatus = MemoryReviewStatus.Accepted,
            ConfidenceScore = 0.97,
            NormalizedKey = "UserFact:the user prefers typescript over javascript",
            SlotKey = "user.preference",
            SlotFamily = MemorySlotFamily.Preference,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var service = CreateService(
            fixture,
            """
            {
              "proposals": [
                {
                  "category": "UserFact",
                  "content": "The user prefers JavaScript over TypeScript.",
                  "slotKey": "user.preference",
                  "slotFamily": "Preference",
                  "confidence": 0.93,
                  "reason": "A new preference statement appears in dialogue.",
                  "evidence": "Actually, I prefer JavaScript these days.",
                  "explicitness": 0.95,
                  "persistence": 0.94,
                  "sceneBound": 0.05,
                  "conflictRisk": 0.85
                }
              ]
            }
            """);

        var result = await service.GenerateForConversationAsync(fixture.ConversationId);

        Assert.Equal(1, result.CreatedProposalCount);
        Assert.Equal(1, result.ConflictingDurableProposalCount);

        Assert.Equal(2, fixture.MemoryRepository.Items.Count);

        var conflicting = fixture.MemoryRepository.Items.Single(x => x.ReviewStatus == MemoryReviewStatus.Proposed);
        Assert.Equal("user.preference", conflicting.SlotKey);
        Assert.Equal(MemorySlotFamily.Preference, conflicting.SlotFamily);
        Assert.NotNull(conflicting.ConflictsWithMemoryItemId);
        Assert.Contains(
            fixture.AuditRepository.Items,
            x => x.Action == "ProposedConflict" || x.Action == "ProposedDurable" || x.Action == "AcceptedDurable");
    }

    private static TestFixture CreateFixture()
    {
        var characterId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            CharacterId = characterId,
            Title = "Memory Test",
            Character = new Character
            {
                Id = characterId,
                Name = "Elena",
                Description = "Test",
                Greeting = "Hi",
                PersonalityDefinition = "Warm",
                Scenario = "Romantic balcony scene",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            Messages = new List<Message>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.User,
                    Content = "Test message.",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = "Test reply.",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow
                }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new TestFixture
        {
            CharacterId = characterId,
            ConversationId = conversationId,
            ConversationRepository = new FakeConversationRepository(conversation),
            MemoryRepository = new FakeMemoryRepository(),
            MemoryOperationAuditService = new FakeMemoryOperationAuditService(),
            SceneStateEventRepository = new FakeSceneStateExtractionEventRepository(),
            AuditRepository = new FakeMemoryExtractionAuditEventRepository()
        };
    }

    private static MemoryProposalService CreateService(
        TestFixture fixture,
        string response,
        MemoryProposalOptions? options = null)
    {
        options ??= new MemoryProposalOptions
        {
            AutoAcceptDurableFacts = false
        };

        return new MemoryProposalService(
            fixture.ConversationRepository,
            fixture.MemoryRepository,
            new FakeInferenceProvider(response),
            new MemoryProposalQualityEvaluator(),
            new MemoryExtractionClassifier(options),
            options,
            new MemoryPolicyService(),
            fixture.MemoryOperationAuditService,
            fixture.SceneStateEventRepository,
            fixture.AuditRepository);
    }

    private sealed class TestFixture
    {
        public required Guid CharacterId { get; init; }

        public required Guid ConversationId { get; init; }

        public required FakeConversationRepository ConversationRepository { get; init; }

        public required FakeMemoryRepository MemoryRepository { get; init; }

        public required FakeMemoryOperationAuditService MemoryOperationAuditService { get; init; }

        public required FakeSceneStateExtractionEventRepository SceneStateEventRepository { get; init; }

        public required FakeMemoryExtractionAuditEventRepository AuditRepository { get; init; }
    }

    private sealed class FakeSceneStateExtractionEventRepository : ISceneStateExtractionEventRepository
    {
        public List<SceneStateExtractionEvent> Items { get; } = new();

        public Task AddAsync(SceneStateExtractionEvent item, CancellationToken cancellationToken = default)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SceneStateExtractionEvent>> ListByConversationAsync(
            Guid conversationId,
            int maxCount = 100,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SceneStateExtractionEvent>>(Items
                .Where(x => x.ConversationId == conversationId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(maxCount)
                .ToList());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeMemoryOperationAuditService : IMemoryOperationAuditService
    {
        public Task<Guid> RecordAsync(
            Guid memoryItemId,
            MemoryOperationType operationType,
            object? beforeState,
            object? afterState,
            Guid? sourceMemoryItemId = null,
            Guid? targetMemoryItemId = null,
            Guid? conversationId = null,
            Guid? characterId = null,
            int? messageSequenceNumber = null,
            string? note = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Guid.NewGuid());
    }

    private sealed class FakeMemoryExtractionAuditEventRepository : IMemoryExtractionAuditEventRepository
    {
        public List<MemoryExtractionAuditEvent> Items { get; } = new();

        public Task AddAsync(MemoryExtractionAuditEvent item, CancellationToken cancellationToken = default)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MemoryExtractionAuditEvent>> ListByConversationAsync(
            Guid conversationId,
            int maxCount = 250,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryExtractionAuditEvent>>(
                Items.Where(x => x.ConversationId == conversationId).Take(maxCount).ToList());

        public Task<int> DeleteOlderThanAsync(DateTime utcCutoff, CancellationToken cancellationToken = default)
        {
            var removed = Items.RemoveAll(x => x.CreatedAt < utcCutoff);
            return Task.FromResult(removed);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Conversation _conversation;

        public FakeConversationRepository(Conversation conversation)
        {
            _conversation = conversation;
        }

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _conversation.Id ? _conversation : null);

        public Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.FromResult<Conversation?>(null);

        public Task<IReadOnlyList<Conversation>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Conversation>>(new[] { _conversation });

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult<SummaryCheckpoint?>(null);

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
            => Task.FromResult(conversation);

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> DeleteMessagesFromSequenceAsync(Guid conversationId, int sequenceNumber, bool inclusive, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task DeleteMessageVariantsAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<int> DeleteSummaryCheckpointsAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeMemoryRepository : IMemoryRepository
    {
        public List<MemoryItem> Items { get; } = new();

        public Task<IReadOnlyList<MemoryItem>> ListAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items);

        public Task<MemoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<MemoryItem>> ListByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items.Where(x => x.ConversationId == conversationId).ToList());

        public Task<IReadOnlyList<MemoryItem>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items.Where(x => x.CharacterId == characterId).ToList());

        public Task<IReadOnlyList<MemoryItem>> ListForProposalComparisonAsync(Guid characterId, Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items.ToList());

        public Task<MemoryItem?> FindActiveByNormalizedKeyAsync(Guid characterId, Guid? conversationId, string normalizedKey, MemoryKind kind, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x =>
                x.CharacterId == characterId &&
                x.ConversationId == conversationId &&
                x.NormalizedKey == normalizedKey &&
                x.Kind == kind &&
                x.ReviewStatus == MemoryReviewStatus.Accepted));

        public Task<MemoryItem?> FindTrackedBySlotAsync(Guid characterId, Guid? conversationId, string slotKey, MemoryKind kind, CancellationToken cancellationToken = default)
            => Task.FromResult(Items
                .Where(x =>
                    x.CharacterId == characterId &&
                    x.ConversationId == conversationId &&
                    x.Kind == kind &&
                    x.SlotKey == slotKey &&
                    x.ReviewStatus != MemoryReviewStatus.Rejected)
                .OrderByDescending(x => x.ReviewStatus == MemoryReviewStatus.Accepted)
                .ThenByDescending(x => x.UpdatedAt)
                .FirstOrDefault());

        public Task<MemoryItem?> FindTrackedByFamilyAsync(Guid characterId, Guid? conversationId, MemorySlotFamily slotFamily, MemoryKind kind, CancellationToken cancellationToken = default)
            => Task.FromResult(Items
                .Where(x =>
                    x.CharacterId == characterId &&
                    x.ConversationId == conversationId &&
                    x.Kind == kind &&
                    x.SlotFamily == slotFamily &&
                    x.ReviewStatus != MemoryReviewStatus.Rejected)
                .OrderByDescending(x => x.ReviewStatus == MemoryReviewStatus.Accepted)
                .ThenByDescending(x => x.UpdatedAt)
                .FirstOrDefault());

        public Task<int> DeleteExpiredSceneStateAsync(DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            var removed = Items.RemoveAll(x => ids.Contains(x.Id));
            return Task.FromResult(removed);
        }

        public Task<MemoryItem> AddAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
        {
            Items.Add(memoryItem);
            return Task.FromResult(memoryItem);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeInferenceProvider : IInferenceProvider
    {
        private readonly string _response;

        public FakeInferenceProvider(string response)
        {
            _response = response;
        }

        public Task<string> StreamCompletionAsync(
            string prompt,
            Func<string, CancellationToken, Task> onToken,
            InferenceExecutionSettings? executionSettings = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_response);
        }
    }
}
