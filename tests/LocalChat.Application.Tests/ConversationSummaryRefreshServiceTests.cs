using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Background;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class ConversationSummaryRefreshServiceTests
{
    [Fact]
    public async Task RefreshAsync_CreatesSummary_WhenEnoughMessagesExist()
    {
        var conversationId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var conversation = BuildConversation(conversationId, agentId, 12);

        var repo = new FakeConversationRepository(conversation);
        var service = new ConversationSummaryRefreshService(
            repo,
            new FakeInferenceProvider("Refreshed summary text."),
            new ConversationBackgroundWorkOptions
            {
                SummaryMinMessagesBeforeRefresh = 10,
                SummaryRecentMessagesToKeepRaw = 4,
                SummaryMinNewMessagesSinceLastRefresh = 2,
                SummaryMaxMessagesInPrompt = 20
            });

        var result = await service.RefreshAsync(conversationId);

        Assert.True(result.Refreshed);
        var checkpoint = Assert.Single(repo.AddedCheckpoints);
        Assert.Equal("Refreshed summary text.", checkpoint.SummaryText);
    }

    [Fact]
    public async Task RefreshAsync_Skips_WhenTooFewNewMessagesSinceLastRefresh()
    {
        var conversationId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var conversation = BuildConversation(conversationId, agentId, 12);

        conversation.SummaryCheckpoints.Add(new SummaryCheckpoint
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            StartSequenceNumber = 1,
            EndSequenceNumber = 7,
            SummaryText = "Existing summary",
            CreatedAt = DateTime.UtcNow
        });

        var repo = new FakeConversationRepository(conversation);
        var service = new ConversationSummaryRefreshService(
            repo,
            new FakeInferenceProvider("Should not be used."),
            new ConversationBackgroundWorkOptions
            {
                SummaryMinMessagesBeforeRefresh = 10,
                SummaryRecentMessagesToKeepRaw = 4,
                SummaryMinNewMessagesSinceLastRefresh = 4,
                SummaryMaxMessagesInPrompt = 20
            });

        var result = await service.RefreshAsync(conversationId);

        Assert.False(result.Refreshed);
        Assert.Contains("Only", result.Reason ?? string.Empty);
        Assert.Empty(repo.AddedCheckpoints);
    }

    private static Conversation BuildConversation(Guid conversationId, Guid agentId, int count)
    {
        var conversation = new Conversation
        {
            Id = conversationId,
            AgentId = agentId,
            Title = "Test",
            Agent = new Agent
            {
                Id = agentId,
                Name = "Elena",
                Description = "Test agent",
                Greeting = "Hi",
                PersonalityDefinition = "Warm",
                Scenario = "Balcony scene",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        for (var i = 1; i <= count; i++)
        {
            conversation.Messages.Add(new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = i % 2 == 0 ? MessageRole.Assistant : MessageRole.User,
                Content = $"Message {i}",
                SequenceNumber = i,
                CreatedAt = DateTime.UtcNow
            });
        }

        return conversation;
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Conversation _conversation;

        public FakeConversationRepository(Conversation conversation)
        {
            _conversation = conversation;
        }

        public List<SummaryCheckpoint> AddedCheckpoints { get; } = new();

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(id == _conversation.Id ? _conversation : null);

        public Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.FromResult<Conversation?>(null);

        public Task<IReadOnlyList<Conversation>> ListByAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Conversation>>(new[] { _conversation });

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(_conversation.SummaryCheckpoints.OrderByDescending(x => x.EndSequenceNumber).FirstOrDefault());

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
            => Task.FromResult(conversation);

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default)
        {
            _conversation.SummaryCheckpoints.Add(checkpoint);
            AddedCheckpoints.Add(checkpoint);
            return Task.CompletedTask;
        }

        public Task<int> DeleteMessagesFromSequenceAsync(Guid conversationId, int sequenceNumber, bool inclusive, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task DeleteMessageVariantsAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<int> DeleteSummaryCheckpointsAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            var count = _conversation.SummaryCheckpoints.Count;
            _conversation.SummaryCheckpoints.Clear();
            return Task.FromResult(count);
        }

        public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult((_conversation.Messages.Max(x => (int?)x.SequenceNumber) ?? 0) + 1);

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
            => Task.FromResult(_response);
    }
}
