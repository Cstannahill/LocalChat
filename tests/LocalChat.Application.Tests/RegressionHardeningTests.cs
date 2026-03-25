using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Chat;
using LocalChat.Application.Features.Chat.SendChatMessage;
using LocalChat.Application.Features.Summaries;
using LocalChat.Application.ImageGeneration;
using LocalChat.Application.Inspection;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.Personas;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class RegressionHardeningTests
{
    [Fact]
    public async Task ContextualImagePrompt_GenerateAsync_ThrowsForMalformedJsonResponse()
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CharacterId = Guid.NewGuid(),
            Character = new Character
            {
                Id = Guid.NewGuid(),
                Name = "Ava",
                PersonalityDefinition = "Calm and observant.",
                Description = "An engineer",
                Scenario = "Night shift",
                Greeting = "Hi"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages =
            [
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = Guid.NewGuid(),
                    Role = MessageRole.User,
                    Content = "Paint the current scene.",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                }
            ]
        };

        var inference = new QueueInferenceProvider(
            """{"positivePrompt":"cinematic street scene""");

        var service = new ConversationVisualPromptService(
            new SingleConversationRepository(conversation),
            new EmptyRetrievalService(),
            inference);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateAsync(conversation.Id));

        Assert.Contains("was not valid JSON", ex.Message);
        Assert.Equal(1, inference.CallCount);
    }

    [Fact]
    public async Task RegenerateLatestAssistantMessageAsync_AllowsContinuationChains()
    {
        var characterId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            Character = new Character
            {
                Id = characterId,
                Name = "Ava",
                PersonalityDefinition = "Calm and observant.",
                Description = "An engineer",
                Scenario = "Night shift",
                Greeting = "Hi"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var user = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "What do you see?",
            SequenceNumber = 1,
            CreatedAt = DateTime.UtcNow
        };

        var assistantFirst = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = "I see a dim control room.",
            SequenceNumber = 2,
            CreatedAt = DateTime.UtcNow,
            SelectedVariantIndex = 0,
            Variants =
            [
                new MessageVariant
                {
                    Id = Guid.NewGuid(),
                    VariantIndex = 0,
                    Content = "I see a dim control room.",
                    CreatedAt = DateTime.UtcNow
                }
            ]
        };

        var assistantLatest = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = "The rain streaks across the windows.",
            SequenceNumber = 3,
            CreatedAt = DateTime.UtcNow,
            SelectedVariantIndex = 0,
            Variants =
            [
                new MessageVariant
                {
                    Id = Guid.NewGuid(),
                    VariantIndex = 0,
                    Content = "The rain streaks across the windows.",
                    CreatedAt = DateTime.UtcNow
                }
            ]
        };

        conversation.Messages.Add(user);
        conversation.Messages.Add(assistantFirst);
        conversation.Messages.Add(assistantLatest);

        var conversationRepository = new SingleConversationRepository(conversation);
        var promptComposer = new CapturingPromptComposer();
        var orchestrator = new ChatOrchestrator(
            new StaticCharacterRepository(conversation.Character!),
            new EmptyUserPersonaRepository(),
            new EmptyModelProfileRepository(),
            new EmptyGenerationPresetRepository(),
            conversationRepository,
            new EmptyRetrievalService(),
            promptComposer,
            new ConstantInferenceProvider("regenerated continuation"),
            new ConstantTokenEstimator(),
            new StaticModelContextService(),
            new EmptyConversationSummaryService(),
            new RecordingConversationBackgroundWorkScheduler(),
            new SummaryOptions { Enabled = false });

        var result = await orchestrator.RegenerateLatestAssistantMessageAsync(
            conversation.Id,
            assistantLatest.Id);

        Assert.Equal(assistantLatest.Id, result.MessageId);
        Assert.Equal("regenerated continuation", result.AssistantMessage);
        Assert.True(promptComposer.LastContext?.ContinueWithoutUserMessage);
        Assert.Equal(1, result.SelectedVariantIndex);
        Assert.Equal(2, result.VariantCount);
        Assert.Equal(1, assistantLatest.SelectedVariantIndex);
        Assert.Equal("regenerated continuation", assistantLatest.Content);
    }

    [Fact]
    public async Task SendAsync_SchedulesBackgroundWork_WithChatTurnCompleteReason()
    {
        var characterId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            CharacterId = characterId,
            Character = new Character
            {
                Id = characterId,
                Name = "Ava",
                PersonalityDefinition = "Calm and observant.",
                Description = "An engineer",
                Scenario = "Night shift",
                Greeting = "Hi"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages =
            [
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.User,
                    Content = "Initial message",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = "Initial response",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow,
                    SelectedVariantIndex = 0
                }
            ]
        };

        var conversationRepository = new SingleConversationRepository(conversation);
        var scheduler = new RecordingConversationBackgroundWorkScheduler();
        var orchestrator = new ChatOrchestrator(
            new StaticCharacterRepository(conversation.Character!),
            new EmptyUserPersonaRepository(),
            new EmptyModelProfileRepository(),
            new EmptyGenerationPresetRepository(),
            conversationRepository,
            new EmptyRetrievalService(),
            new CapturingPromptComposer(),
            new ConstantInferenceProvider("Assistant follow-up"),
            new ConstantTokenEstimator(),
            new StaticModelContextService(),
            new EmptyConversationSummaryService(),
            scheduler,
            new SummaryOptions { Enabled = false });

        var result = await orchestrator.SendAsync(
            new SendChatMessageCommand
            {
                CharacterId = characterId,
                ConversationId = conversationId,
                Message = "Next user message"
            },
            static (_, _) => Task.CompletedTask);

        Assert.Equal(1, scheduler.CallCount);
        Assert.Equal(result.ConversationId, scheduler.LastConversationId);
        Assert.Equal(ConversationBackgroundWorkType.All, scheduler.LastWorkType);
        Assert.Equal("chat-turn-complete", scheduler.LastReason);
    }

    private sealed class QueueInferenceProvider(params string[] responses) : IInferenceProvider
    {
        private readonly Queue<string> _responses = new(responses);

        public int CallCount { get; private set; }

        public Task<string> StreamCompletionAsync(
            string prompt,
            Func<string, CancellationToken, Task> onDelta,
            InferenceExecutionSettings? executionSettings = null,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : string.Empty);
        }
    }

    private sealed class ConstantInferenceProvider(string output) : IInferenceProvider
    {
        public Task<string> StreamCompletionAsync(
            string prompt,
            Func<string, CancellationToken, Task> onDelta,
            InferenceExecutionSettings? executionSettings = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(output);
        }
    }

    private sealed class CapturingPromptComposer : IPromptComposer
    {
        public PromptCompositionContext? LastContext { get; private set; }

        public PromptCompositionResult Compose(PromptCompositionContext context)
        {
            LastContext = context;
            return new PromptCompositionResult
            {
                Prompt = "test prompt",
                Sections =
                [
                    new PromptSection
                    {
                        Name = "Test",
                        Content = "test",
                        EstimatedTokens = 1
                    }
                ],
                SelectedSceneState = Array.Empty<PromptSceneStateSelectedDebugItem>(),
                SuppressedSceneState = Array.Empty<PromptSceneStateSuppressedDebugItem>(),
                SelectedDurableMemory = Array.Empty<PromptDurableMemorySelectedDebugItem>(),
                SuppressedDurableMemory = Array.Empty<PromptDurableMemorySuppressedDebugItem>()
            };
        }
    }

    private sealed class SingleConversationRepository(Conversation conversation) : IConversationRepository
    {
        private readonly Conversation _conversation = conversation;

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == _conversation.Id ? _conversation : null);
        }

        public Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Conversation?>(null);
        }

        public Task<IReadOnlyList<Conversation>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Conversation>>([]);
        }

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SummaryCheckpoint?>(null);
        }

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

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

        public Task<int> DeleteMessagesFromSequenceAsync(
            Guid conversationId,
            int sequenceNumber,
            bool inclusive,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task DeleteMessageVariantsAsync(
            Guid messageId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<int> DeleteSummaryCheckpointsAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_conversation.Messages.Count == 0
                ? 1
                : _conversation.Messages.Max(x => x.SequenceNumber) + 1);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class EmptyRetrievalService : IRetrievalService
    {
        public Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task IndexLoreEntryAsync(Guid characterId, LoreEntry loreEntry, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveSourceAsync(string sourceType, Guid sourceEntityId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<RetrievalInspectionResult> InspectAsync(Guid characterId, Guid? conversationId, string query, CancellationToken cancellationToken = default)
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
        {
            return Task.FromResult<ModelProfile?>(null);
        }

        public Task<IReadOnlyList<ModelProfile>> ListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ModelProfile>>([]);
        }

        public Task<ModelProfile> AddAsync(ModelProfile modelProfile, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(ModelProfile modelProfile)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class EmptyGenerationPresetRepository : IGenerationPresetRepository
    {
        public Task<GenerationPreset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<GenerationPreset?>(null);
        }

        public Task<IReadOnlyList<GenerationPreset>> ListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<GenerationPreset>>([]);
        }

        public Task<GenerationPreset> AddAsync(GenerationPreset generationPreset, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(GenerationPreset generationPreset)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class StaticCharacterRepository(Character character) : ICharacterRepository
    {
        public Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == character.Id ? character : null);
        }

        public Task<Character?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == character.Id ? character : null);
        }

        public Task<Character?> GetDefaultAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Character?>(character);
        }

        public Task<IReadOnlyList<Character>> ListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Character>>([character]);
        }

        public Task<Character> AddAsync(Character character, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> HasConversationsAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(Character character)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class EmptyUserPersonaRepository : IUserPersonaRepository
    {
        public Task<UserPersona?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UserPersona?>(null);
        }

        public Task<IReadOnlyList<UserPersona>> ListAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<UserPersona>>([]);
        }

        public Task<UserPersona> AddAsync(UserPersona persona, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(UserPersona persona)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class ConstantTokenEstimator : ITokenEstimator
    {
        public int EstimateTokens(string? text) => 1;
    }

    private sealed class StaticModelContextService : IModelContextService
    {
        private static readonly ModelContextInfo Context = new()
        {
            ModelName = "test-model",
            EffectiveContextLength = 8192,
            ReservedOutputTokens = 1024,
            SafetyMarginTokens = 128,
            MaxPromptTokens = 7168
        };

        public Task<ModelContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Context);
        }

        public Task<ModelContextInfo> GetForModelAsync(string? modelIdentifier, int? contextWindowOverride, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Context);
        }
    }

    private sealed class EmptyConversationSummaryService : IConversationSummaryService
    {
        public Task<string> BuildRollingSummaryAsync(string? existingSummary, IReadOnlyList<Message> messagesToSummarize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(existingSummary ?? string.Empty);
        }
    }
}
