using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Chat;
using LocalChat.Application.Inspection;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class ConversationContinuationServiceTests
{
    [Fact]
    public async Task ContinueConversationAsync_AppendsAssistantMessage_AndSchedulesBackgroundWork()
    {
        var agentId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            AgentId = agentId,
            Title = "Test Conversation",
            Agent = new Agent
            {
                Id = agentId,
                Name = "Cassandra",
                Description = "Test agent",
                Greeting = "Hi",
                PersonalityDefinition = "Immersive",
                Scenario = "Bar scene",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<Message>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.User,
                    Content = "Hello there.",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = "*She smiles warmly.* \"Hello.\"",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow,
                    SelectedVariantIndex = 0
                }
            }
        };

        var repo = new FakeConversationRepository(conversation);
        var scheduler = new RecordingConversationBackgroundWorkScheduler();
        var service = new ConversationContinuationService(
            repo,
            new EmptyModelProfileRepository(),
            new EmptyGenerationPresetRepository(),
            new FakePromptComposer(),
            new FakeInferenceProvider("She leans a little closer, her smile turning playful."),
            new FakeRetrievalService(),
            scheduler);

        var deltas = new List<string>();

        var result = await service.ContinueConversationAsync(
            conversationId,
            (delta, _) =>
            {
                deltas.Add(delta);
                return Task.CompletedTask;
            });

        Assert.Equal(conversationId, result.ConversationId);
        Assert.Equal("She leans a little closer, her smile turning playful.", result.AssistantMessage);

        Assert.Equal(3, conversation.Messages.Count);
        Assert.Equal(1, conversation.Messages.Count(x => x.Role == MessageRole.User));
        Assert.Equal(2, conversation.Messages.Count(x => x.Role == MessageRole.Assistant));

        var lastMessage = conversation.Messages.OrderBy(x => x.SequenceNumber).Last();
        Assert.Equal(MessageRole.Assistant, lastMessage.Role);
        Assert.Equal(3, lastMessage.SequenceNumber);
        Assert.Equal("She leans a little closer, her smile turning playful.", lastMessage.Content);

        Assert.NotEmpty(deltas);
        Assert.Equal(1, scheduler.CallCount);
        Assert.Equal(conversationId, scheduler.LastConversationId);
        Assert.Equal(ConversationBackgroundWorkType.All, scheduler.LastWorkType);
        Assert.Equal("conversation-continue", scheduler.LastReason);
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
            => Task.FromResult(_conversation.Messages.Any(x => x.Id == messageId) ? _conversation : null);

        public Task<IReadOnlyList<Conversation>> ListByAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Conversation>>(new[] { _conversation });

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult<SummaryCheckpoint?>(null);

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
            => Task.CompletedTask;

        public Task<int> DeleteMessagesFromSequenceAsync(
            Guid conversationId,
            int sequenceNumber,
            bool inclusive,
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task DeleteMessageVariantsAsync(
            Guid messageId,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<int> DeleteSummaryCheckpointsAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(_conversation.Messages.Max(x => x.SequenceNumber) + 1);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakePromptComposer : IPromptComposer
    {
        public PromptCompositionResult Compose(PromptCompositionContext context)
        {
            return new PromptCompositionResult
            {
                Prompt = "continue prompt",
                Sections = Array.Empty<PromptSection>(),
                SelectedSessionState = Array.Empty<PromptSessionStateSelectedDebugItem>(),
                SuppressedSessionState = Array.Empty<PromptSessionStateSuppressedDebugItem>(),
                SelectedDurableMemory = Array.Empty<PromptDurableMemorySelectedDebugItem>(),
                SuppressedDurableMemory = Array.Empty<PromptDurableMemorySuppressedDebugItem>()
            };
        }
    }

    private sealed class FakeInferenceProvider : IInferenceProvider
    {
        private readonly string _response;

        public FakeInferenceProvider(string response)
        {
            _response = response;
        }

        public async Task<string> StreamCompletionAsync(
            string prompt,
            Func<string, CancellationToken, Task> onToken,
            InferenceExecutionSettings? executionSettings = null,
            CancellationToken cancellationToken = default)
        {
            await onToken(_response, cancellationToken);
            return _response;
        }
    }

    private sealed class FakeRetrievalService : IRetrievalService
    {
        public Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task IndexLoreEntryAsync(
            Guid agentId,
            LoreEntry loreEntry,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveSourceAsync(
            string sourceType,
            Guid sourceEntityId,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<RetrievalInspectionResult> InspectAsync(
            Guid agentId,
            Guid? conversationId,
            string query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RetrievalInspectionResult
            {
                Query = query,
                SelectedMemories = Array.Empty<MemoryItem>(),
                SelectedLoreEntries = Array.Empty<LoreEntry>(),
                SelectedMemoryExplanations = Array.Empty<SelectedMemoryExplanation>(),
                SelectedLoreExplanations = Array.Empty<SelectedLoreExplanation>()
            });
        }
    }

    private sealed class EmptyModelProfileRepository : IModelProfileRepository
    {
        public Task<ModelProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<ModelProfile?>(null);

        public Task<IReadOnlyList<ModelProfile>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ModelProfile>>(Array.Empty<ModelProfile>());

        public Task<ModelProfile> AddAsync(ModelProfile modelProfile, CancellationToken cancellationToken = default)
            => Task.FromResult(modelProfile);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(ModelProfile modelProfile)
        {
        }
    }

    private sealed class EmptyGenerationPresetRepository : IGenerationPresetRepository
    {
        public Task<GenerationPreset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<GenerationPreset?>(null);

        public Task<IReadOnlyList<GenerationPreset>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GenerationPreset>>(Array.Empty<GenerationPreset>());

        public Task<GenerationPreset> AddAsync(GenerationPreset generationPreset, CancellationToken cancellationToken = default)
            => Task.FromResult(generationPreset);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(GenerationPreset generationPreset)
        {
        }
    }
}
