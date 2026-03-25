using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Chat;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class ConversationMessageMutationServiceTests
{
    [Fact]
    public async Task EditAssistantMessage_TruncatesLaterMessages_InvalidatesSummaries_Reindexes_AndSchedulesDeferredWork()
    {
        var fixture = CreateFixtureWithConversation(
            ("User", "User opening"),
            ("Assistant", "Assistant original"),
            ("User", "Later user"),
            ("Assistant", "Later assistant"));

        var assistantMessage = fixture.Conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .First(x => x.Role == MessageRole.Assistant);

        assistantMessage.Variants.Add(new MessageVariant
        {
            Id = Guid.NewGuid(),
            MessageId = assistantMessage.Id,
            VariantIndex = 0,
            Content = "Assistant original",
            CreatedAt = DateTime.UtcNow
        });

        assistantMessage.Variants.Add(new MessageVariant
        {
            Id = Guid.NewGuid(),
            MessageId = assistantMessage.Id,
            VariantIndex = 1,
            Content = "Assistant alternate",
            CreatedAt = DateTime.UtcNow
        });

        fixture.Conversation.SummaryCheckpoints.Add(new SummaryCheckpoint
        {
            Id = Guid.NewGuid(),
            ConversationId = fixture.Conversation.Id,
            StartSequenceNumber = 1,
            EndSequenceNumber = 4,
            SummaryText = "Old summary",
            CreatedAt = DateTime.UtcNow
        });

        var service = CreateService(fixture);

        var result = await service.EditAsync(
            fixture.Conversation.Id,
            assistantMessage.Id,
            "Edited assistant message",
            regenerateAssistant: true);

        Assert.Equal("Edit", result.Operation);
        Assert.Equal(2, result.DeletedMessageCount);
        Assert.True(result.SummariesInvalidated);
        Assert.True(result.RetrievalReindexed);
        Assert.False(result.AssistantRegenerated);

        var ordered = fixture.Conversation.Messages.OrderBy(x => x.SequenceNumber).ToList();
        Assert.Equal(2, ordered.Count);
        Assert.Equal("User opening", ordered[0].Content);
        Assert.Equal("Edited assistant message", ordered[1].Content);

        var editedAssistant = ordered[1];
        Assert.Single(editedAssistant.Variants);
        Assert.Equal("Edited assistant message", editedAssistant.Variants.Single().Content);

        Assert.Equal(1, fixture.Repository.DeleteSummaryCheckpointsCallCount);
        Assert.Equal(1, fixture.RetrievalSync.ReindexCallCount);
        Assert.Equal(0, fixture.AssistantGenerator.GenerateCallCount);

        Assert.Equal(1, fixture.Scheduler.CallCount);
        Assert.Equal("message-edit", fixture.Scheduler.LastReason);
        Assert.Equal(
            ConversationBackgroundWorkType.MemoryExtraction | ConversationBackgroundWorkType.SummaryRefresh,
            fixture.Scheduler.LastWorkType);
    }

    [Fact]
    public async Task DeleteLatestAssistantMessage_WithRegenerate_RegeneratesFromLatestUser_AndSchedulesDeferredWork()
    {
        var fixture = CreateFixtureWithConversation(
            ("User", "User opening"),
            ("Assistant", "Assistant reply"));

        fixture.Conversation.SummaryCheckpoints.Add(new SummaryCheckpoint
        {
            Id = Guid.NewGuid(),
            ConversationId = fixture.Conversation.Id,
            StartSequenceNumber = 1,
            EndSequenceNumber = 2,
            SummaryText = "Old summary",
            CreatedAt = DateTime.UtcNow
        });

        var latestAssistant = fixture.Conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .Last();

        fixture.AssistantGenerator.NextResult = new GeneratedAssistantTurnResult
        {
            ConversationId = fixture.Conversation.Id,
            AssistantMessageId = Guid.NewGuid(),
            AssistantMessage = "Regenerated assistant reply"
        };

        var service = CreateService(fixture);

        var result = await service.DeleteAsync(
            fixture.Conversation.Id,
            latestAssistant.Id,
            regenerateAssistant: true);

        Assert.Equal("Delete", result.Operation);
        Assert.Equal(1, result.DeletedMessageCount);
        Assert.True(result.SummariesInvalidated);
        Assert.True(result.AssistantRegenerated);
        Assert.True(result.RetrievalReindexed);

        Assert.Equal(1, fixture.AssistantGenerator.GenerateCallCount);
        Assert.Equal(1, fixture.Repository.DeleteSummaryCheckpointsCallCount);
        Assert.Equal(1, fixture.RetrievalSync.ReindexCallCount);

        Assert.Equal(1, fixture.Scheduler.CallCount);
        Assert.Equal("message-delete", fixture.Scheduler.LastReason);
        Assert.Equal(
            ConversationBackgroundWorkType.MemoryExtraction | ConversationBackgroundWorkType.SummaryRefresh,
            fixture.Scheduler.LastWorkType);
    }

    [Fact]
    public async Task DeleteFirstMessage_Throws_AndDoesNotMutate()
    {
        var fixture = CreateFixtureWithConversation(
            ("User", "First"),
            ("Assistant", "Second"));

        var firstMessage = fixture.Conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .First();

        var service = CreateService(fixture);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(
                fixture.Conversation.Id,
                firstMessage.Id,
                regenerateAssistant: false));

        Assert.Contains("Cannot delete the first message", ex.Message);
        Assert.Equal(0, fixture.Repository.DeleteSummaryCheckpointsCallCount);
        Assert.Equal(0, fixture.RetrievalSync.ReindexCallCount);
        Assert.Equal(0, fixture.AssistantGenerator.GenerateCallCount);
        Assert.Equal(0, fixture.Scheduler.CallCount);
        Assert.Equal(2, fixture.Conversation.Messages.Count);
    }

    [Fact]
    public async Task DeleteOnlyRemainingMessage_Throws_AndDoesNotMutate()
    {
        var fixture = CreateFixtureWithConversation(
            ("User", "Only message"));

        var onlyMessage = fixture.Conversation.Messages.Single();

        var service = CreateService(fixture);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(
                fixture.Conversation.Id,
                onlyMessage.Id,
                regenerateAssistant: false));

        Assert.Contains("only remaining message", ex.Message);
        Assert.Equal(0, fixture.Repository.DeleteSummaryCheckpointsCallCount);
        Assert.Equal(0, fixture.RetrievalSync.ReindexCallCount);
        Assert.Equal(0, fixture.AssistantGenerator.GenerateCallCount);
        Assert.Equal(0, fixture.Scheduler.CallCount);
        Assert.Single(fixture.Conversation.Messages);
    }

    private static ConversationMessageMutationService CreateService(TestFixture fixture)
    {
        return new ConversationMessageMutationService(
            fixture.Repository,
            fixture.AssistantGenerator,
            fixture.RetrievalSync,
            fixture.Scheduler);
    }

    private static TestFixture CreateFixtureWithConversation(params (string Role, string Content)[] messages)
    {
        var characterId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            CharacterId = characterId,
            Title = "Old title",
            Character = new Character
            {
                Id = characterId,
                Name = "Cassandra",
                Description = "Test",
                Greeting = "Hi",
                PersonalityDefinition = "Immersive",
                Scenario = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var sequence = 1;
        foreach (var (role, content) in messages)
        {
            conversation.Messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = Enum.Parse<MessageRole>(role, ignoreCase: true),
                Content = content,
                SequenceNumber = sequence++,
                CreatedAt = DateTime.UtcNow,
                SelectedVariantIndex = 0
            });
        }

        var repository = new FakeConversationRepository(conversation);
        var generator = new FakeAssistantTurnGenerationService(conversation);
        var retrievalSync = new FakeConversationRetrievalSyncService();
        var scheduler = new FakeConversationBackgroundWorkScheduler();

        return new TestFixture
        {
            Conversation = conversation,
            Repository = repository,
            AssistantGenerator = generator,
            RetrievalSync = retrievalSync,
            Scheduler = scheduler
        };
    }

    private sealed class TestFixture
    {
        public required Conversation Conversation { get; init; }

        public required FakeConversationRepository Repository { get; init; }

        public required FakeAssistantTurnGenerationService AssistantGenerator { get; init; }

        public required FakeConversationRetrievalSyncService RetrievalSync { get; init; }

        public required FakeConversationBackgroundWorkScheduler Scheduler { get; init; }
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Conversation _conversation;

        public FakeConversationRepository(Conversation conversation)
        {
            _conversation = conversation;
        }

        public int DeleteSummaryCheckpointsCallCount { get; private set; }

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _conversation.Id ? _conversation : null);

        public Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.FromResult(_conversation.Messages.Any(x => x.Id == messageId) ? _conversation : null);

        public Task<IReadOnlyList<Conversation>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Conversation>>(new[] { _conversation });

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(_conversation.SummaryCheckpoints.OrderByDescending(x => x.EndSequenceNumber).FirstOrDefault());

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
            => Task.FromResult(conversation);

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            _conversation.Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default)
        {
            var message = _conversation.Messages.First(x => x.Id == variant.MessageId);
            message.Variants.Add(variant);
            return Task.CompletedTask;
        }

        public Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default)
        {
            _conversation.SummaryCheckpoints.Add(checkpoint);
            return Task.CompletedTask;
        }

        public Task<int> DeleteMessagesFromSequenceAsync(Guid conversationId, int sequenceNumber, bool inclusive, CancellationToken cancellationToken = default)
        {
            var toRemove = _conversation.Messages
                .Where(x => inclusive ? x.SequenceNumber >= sequenceNumber : x.SequenceNumber > sequenceNumber)
                .ToList();

            foreach (var item in toRemove)
            {
                _conversation.Messages.Remove(item);
            }

            return Task.FromResult(toRemove.Count);
        }

        public Task DeleteMessageVariantsAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            var message = _conversation.Messages.FirstOrDefault(x => x.Id == messageId);
            message?.Variants.Clear();
            return Task.CompletedTask;
        }

        public Task<int> DeleteSummaryCheckpointsAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            DeleteSummaryCheckpointsCallCount++;

            var count = _conversation.SummaryCheckpoints.Count;
            _conversation.SummaryCheckpoints.Clear();
            return Task.FromResult(count);
        }

        public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult((_conversation.Messages.Max(x => (int?)x.SequenceNumber) ?? 0) + 1);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeAssistantTurnGenerationService : IAssistantTurnGenerationService
    {
        private readonly Conversation _conversation;

        public FakeAssistantTurnGenerationService(Conversation conversation)
        {
            _conversation = conversation;
        }

        public int GenerateCallCount { get; private set; }

        public GeneratedAssistantTurnResult? NextResult { get; set; }

        public Task<GeneratedAssistantTurnResult> GenerateAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            GenerateCallCount++;

            var result = NextResult ?? new GeneratedAssistantTurnResult
            {
                ConversationId = conversationId,
                AssistantMessageId = Guid.NewGuid(),
                AssistantMessage = "Regenerated assistant reply"
            };

            var nextSequence = (_conversation.Messages.Max(x => (int?)x.SequenceNumber) ?? 0) + 1;

            var assistantMessage = new Message
            {
                Id = result.AssistantMessageId,
                ConversationId = conversationId,
                Role = MessageRole.Assistant,
                Content = result.AssistantMessage,
                SequenceNumber = nextSequence,
                CreatedAt = DateTime.UtcNow,
                SelectedVariantIndex = 0
            };

            assistantMessage.Variants.Add(new MessageVariant
            {
                Id = Guid.NewGuid(),
                MessageId = assistantMessage.Id,
                VariantIndex = 0,
                Content = assistantMessage.Content,
                CreatedAt = DateTime.UtcNow
            });

            _conversation.Messages.Add(assistantMessage);
            _conversation.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(result);
        }
    }

    private sealed class FakeConversationRetrievalSyncService : IConversationRetrievalSyncService
    {
        public int ReindexCallCount { get; private set; }

        public Task ReindexConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            ReindexCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeConversationBackgroundWorkScheduler : IConversationBackgroundWorkScheduler
    {
        public int CallCount { get; private set; }

        public ConversationBackgroundWorkType LastWorkType { get; private set; }

        public string? LastReason { get; private set; }

        public Task ScheduleConversationChangeAsync(
            Guid conversationId,
            ConversationBackgroundWorkType workType,
            string reason,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastWorkType = workType;
            LastReason = reason;
            return Task.CompletedTask;
        }
    }
}
