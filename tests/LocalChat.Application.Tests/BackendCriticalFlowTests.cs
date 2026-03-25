using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Background;
using LocalChat.Application.Chat;
using LocalChat.Application.Features.Chat.SendChatMessage;
using LocalChat.Application.Features.Summaries;
using LocalChat.Application.Inspection;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Application.Runtime;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Generation;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.Personas;
using LocalChat.Domain.Entities.Settings;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class BackendCriticalFlowTests
{
    [Fact]
    public async Task SendAsync_NewConversation_SeedsGreeting_AndPreservesGreetingUserAssistantOrder()
    {
        var character = BuildCharacter();
        var repository = new InMemoryConversationRepository();
        var snapshots = new RecordingGenerationPromptSnapshotRepository();

        var orchestrator = BuildChatOrchestrator(character, repository, snapshots, "Assistant reply");

        var result = await orchestrator.SendAsync(
            new SendChatMessageCommand
            {
                CharacterId = character.Id,
                Message = "First user message"
            },
            static (_, _) => Task.CompletedTask);

        var conversation = await repository.GetByIdWithMessagesAsync(result.ConversationId);
        Assert.NotNull(conversation);

        var ordered = conversation!.Messages.OrderBy(x => x.SequenceNumber).ToList();
        Assert.Equal(3, ordered.Count);

        Assert.Equal(MessageRole.Assistant, ordered[0].Role);
        Assert.Equal(MessageOriginType.SeedGreeting, ordered[0].OriginType);
        Assert.Equal("Hello from seed", ordered[0].Content);

        Assert.Equal(MessageRole.User, ordered[1].Role);
        Assert.Equal(MessageOriginType.User, ordered[1].OriginType);
        Assert.Equal("First user message", ordered[1].Content);

        Assert.Equal(MessageRole.Assistant, ordered[2].Role);
        Assert.Equal(MessageOriginType.AssistantGenerated, ordered[2].OriginType);
        Assert.Equal("Assistant reply", ordered[2].Content);

        Assert.Single(snapshots.Snapshots);
    }

    [Fact]
    public async Task RuntimeSelectionResolver_Resolves_OneTurn_ThenSticky_ThenCharacter_ThenApp_ThenProviderDefault()
    {
        var appProfile = new ModelProfile
        {
            Id = Guid.NewGuid(),
            Name = "App",
            ProviderType = ProviderType.Ollama,
            ModelIdentifier = "ollama:app-model",
            ContextWindow = 4096,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var characterProfile = new ModelProfile
        {
            Id = Guid.NewGuid(),
            Name = "Character",
            ProviderType = ProviderType.OpenRouter,
            ModelIdentifier = "openrouter:character/model",
            ContextWindow = 8192,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var stickyProfile = new ModelProfile
        {
            Id = Guid.NewGuid(),
            Name = "Sticky",
            ProviderType = ProviderType.HuggingFace,
            ModelIdentifier = "hf:sticky/model",
            ContextWindow = 12288,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var generationPreset = new GenerationPreset
        {
            Id = Guid.NewGuid(),
            Name = "Preset",
            MaxOutputTokens = 256,
            Temperature = 0.7,
            TopP = 0.95,
            RepeatPenalty = 1.05,
            StopSequencesText = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var appDefaults = new AppRuntimeDefaults
        {
            Id = Guid.NewGuid(),
            DefaultModelProfileId = appProfile.Id,
            DefaultGenerationPresetId = generationPreset.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var resolver = new RuntimeSelectionResolver(
            new StaticAppRuntimeDefaultsRepository(appDefaults),
            new StaticModelProfileRepository(appProfile, characterProfile, stickyProfile),
            new StaticGenerationPresetRepository(generationPreset));

        var character = BuildCharacter();
        character.DefaultModelProfileId = characterProfile.Id;
        character.DefaultGenerationPresetId = generationPreset.Id;

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            RuntimeModelProfileOverrideId = stickyProfile.Id,
            RuntimeGenerationPresetOverrideId = generationPreset.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var oneTurn = await resolver.ResolveAsync(character, conversation, "ollama", "one-turn-model");
        Assert.Equal(RuntimeSourceType.OneTurnOverride, oneTurn.SourceType);
        Assert.Equal("ollama:one-turn-model", oneTurn.ModelIdentifier);

        var sticky = await resolver.ResolveAsync(character, conversation, null, null);
        Assert.Equal(RuntimeSourceType.ConversationStickyOverride, sticky.SourceType);
        Assert.Equal(stickyProfile.ModelIdentifier, sticky.ModelIdentifier);

        conversation.RuntimeModelProfileOverrideId = null;
        conversation.RuntimeGenerationPresetOverrideId = null;
        var characterDefault = await resolver.ResolveAsync(character, conversation, null, null);
        Assert.Equal(RuntimeSourceType.CharacterDefault, characterDefault.SourceType);
        Assert.Equal(characterProfile.ModelIdentifier, characterDefault.ModelIdentifier);

        character.DefaultModelProfileId = null;
        character.DefaultGenerationPresetId = null;
        var appDefault = await resolver.ResolveAsync(character, conversation, null, null);
        Assert.Equal(RuntimeSourceType.AppDefault, appDefault.SourceType);
        Assert.Equal(appProfile.ModelIdentifier, appDefault.ModelIdentifier);

        appDefaults.DefaultModelProfileId = null;
        appDefaults.DefaultGenerationPresetId = null;
        var providerDefault = await resolver.ResolveAsync(character, conversation, null, null);
        Assert.Equal(RuntimeSourceType.ProviderDefault, providerDefault.SourceType);
        Assert.Null(providerDefault.ModelIdentifier);
    }

    [Fact]
    public async Task ContinueConversationAsync_PersistsPromptSnapshot()
    {
        var character = BuildCharacter();
        var conversation = BuildConversationWithUserAssistant(character);
        var repository = new InMemoryConversationRepository(conversation);
        var snapshots = new RecordingGenerationPromptSnapshotRepository();

        var service = new ConversationContinuationService(
            repository,
            new StaticModelProfileRepository(),
            new StaticGenerationPresetRepository(),
            new SimplePromptComposer(),
            new ConstantInferenceProvider("Continuation reply"),
            new EmptyRetrievalService(),
            new RecordingConversationBackgroundWorkScheduler(),
            runtimeSelectionResolver: null,
            generationPromptSnapshotRepository: snapshots);

        await service.ContinueConversationAsync(conversation.Id, static (_, _) => Task.CompletedTask);

        var snapshot = Assert.Single(snapshots.Snapshots);
        Assert.Equal(conversation.Id, snapshot.ConversationId);
        Assert.Equal("prompt-body", snapshot.FullPromptText);
    }

    [Fact]
    public async Task RegenerateLatestAssistantMessageAsync_PersistsPromptSnapshot()
    {
        var character = BuildCharacter();
        var conversation = BuildConversationWithUserAssistant(character);
        var repository = new InMemoryConversationRepository(conversation);
        var snapshots = new RecordingGenerationPromptSnapshotRepository();

        var orchestrator = BuildChatOrchestrator(character, repository, snapshots, "Regenerated reply");
        var latestAssistant = conversation.Messages.OrderBy(x => x.SequenceNumber).Last();

        var result = await orchestrator.RegenerateLatestAssistantMessageAsync(conversation.Id, latestAssistant.Id);

        var snapshot = Assert.Single(snapshots.Snapshots);
        Assert.Equal(conversation.Id, snapshot.ConversationId);
        Assert.Equal(result.MessageId, snapshot.MessageId);
        Assert.Equal("prompt-body", snapshot.FullPromptText);
    }

    private static ChatOrchestrator BuildChatOrchestrator(
        Character character,
        InMemoryConversationRepository repository,
        RecordingGenerationPromptSnapshotRepository snapshots,
        string assistantReply)
    {
        return new ChatOrchestrator(
            new StaticCharacterRepository(character),
            new StaticUserPersonaRepository(),
            new StaticModelProfileRepository(),
            new StaticGenerationPresetRepository(),
            repository,
            new EmptyRetrievalService(),
            new SimplePromptComposer(),
            new ConstantInferenceProvider(assistantReply),
            new ConstantTokenEstimator(),
            new StaticModelContextService(),
            new PassthroughConversationSummaryService(),
            new RecordingConversationBackgroundWorkScheduler(),
            new SummaryOptions { Enabled = false },
            runtimeSelectionResolver: null,
            generationPromptSnapshotRepository: snapshots);
    }

    private static Character BuildCharacter()
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "Ava",
            Description = "Test",
            Greeting = "Hello from seed",
            PersonalityDefinition = "Calm",
            Scenario = "Testing",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Conversation BuildConversationWithUserAssistant(Character character)
    {
        var conversationId = Guid.NewGuid();
        return new Conversation
        {
            Id = conversationId,
            CharacterId = character.Id,
            Character = character,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages =
            [
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.User,
                    OriginType = MessageOriginType.User,
                    Content = "User says hi",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    OriginType = MessageOriginType.AssistantGenerated,
                    Content = "Assistant says hi",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow,
                    SelectedVariantIndex = 0,
                    Variants =
                    [
                        new MessageVariant
                        {
                            Id = Guid.NewGuid(),
                            VariantIndex = 0,
                            Content = "Assistant says hi",
                            CreatedAt = DateTime.UtcNow
                        }
                    ]
                }
            ]
        };
    }

    private sealed class InMemoryConversationRepository : IConversationRepository
    {
        private readonly Dictionary<Guid, Conversation> _conversations;

        public InMemoryConversationRepository(params Conversation[] seed)
        {
            _conversations = seed.ToDictionary(x => x.Id, x => x);
        }

        public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_conversations.TryGetValue(id, out var conversation) ? conversation : null);

        public Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.FromResult(_conversations.Values.FirstOrDefault(x => x.Messages.Any(m => m.Id == messageId)));

        public Task<IReadOnlyList<Conversation>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Conversation>>(_conversations.Values.Where(x => x.CharacterId == characterId).ToList());

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            if (!_conversations.TryGetValue(conversationId, out var conversation))
            {
                return Task.FromResult<SummaryCheckpoint?>(null);
            }

            var latest = conversation.SummaryCheckpoints
                .OrderByDescending(x => x.EndSequenceNumber)
                .FirstOrDefault();
            return Task.FromResult(latest);
        }

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            _conversations[conversation.Id] = conversation;
            return Task.FromResult(conversation);
        }

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            _conversations[message.ConversationId].Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default)
        {
            var conversation = _conversations.Values.First(x => x.Messages.Any(m => m.Id == variant.MessageId));
            var message = conversation.Messages.First(x => x.Id == variant.MessageId);
            message.Variants.Add(variant);
            return Task.CompletedTask;
        }

        public Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default)
        {
            _conversations[checkpoint.ConversationId].SummaryCheckpoints.Add(checkpoint);
            return Task.CompletedTask;
        }

        public Task<int> DeleteMessagesFromSequenceAsync(Guid conversationId, int sequenceNumber, bool inclusive, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task DeleteMessageVariantsAsync(Guid messageId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<int> DeleteSummaryCheckpointsAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            var next = _conversations[conversationId].Messages.Max(x => x.SequenceNumber) + 1;
            return Task.FromResult(next);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StaticCharacterRepository(params Character[] characters) : ICharacterRepository
    {
        private readonly Dictionary<Guid, Character> _characters = characters.ToDictionary(x => x.Id, x => x);

        public Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_characters.TryGetValue(id, out var character) ? character : null);

        public Task<Character?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<Character?> GetDefaultAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Character?>(_characters.Values.FirstOrDefault());

        public Task<IReadOnlyList<Character>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Character>>(_characters.Values.ToList());

        public Task<Character> AddAsync(Character character, CancellationToken cancellationToken = default)
            => Task.FromResult(character);

        public Task<bool> HasConversationsAsync(Guid characterId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(Character character)
        {
        }
    }

    private sealed class StaticUserPersonaRepository : IUserPersonaRepository
    {
        public Task<UserPersona?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<UserPersona?>(null);

        public Task<IReadOnlyList<UserPersona>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserPersona>>([]);

        public Task<UserPersona> AddAsync(UserPersona persona, CancellationToken cancellationToken = default)
            => Task.FromResult(persona);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(UserPersona persona)
        {
        }
    }

    private sealed class StaticModelProfileRepository(params ModelProfile[] profiles) : IModelProfileRepository
    {
        private readonly Dictionary<Guid, ModelProfile> _profiles = profiles.ToDictionary(x => x.Id, x => x);

        public Task<ModelProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_profiles.TryGetValue(id, out var profile) ? profile : null);

        public Task<IReadOnlyList<ModelProfile>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ModelProfile>>(_profiles.Values.ToList());

        public Task<ModelProfile> AddAsync(ModelProfile modelProfile, CancellationToken cancellationToken = default)
            => Task.FromResult(modelProfile);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(ModelProfile modelProfile)
        {
        }
    }

    private sealed class StaticGenerationPresetRepository(params GenerationPreset[] presets) : IGenerationPresetRepository
    {
        private readonly Dictionary<Guid, GenerationPreset> _presets = presets.ToDictionary(x => x.Id, x => x);

        public Task<GenerationPreset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_presets.TryGetValue(id, out var preset) ? preset : null);

        public Task<IReadOnlyList<GenerationPreset>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GenerationPreset>>(_presets.Values.ToList());

        public Task<GenerationPreset> AddAsync(GenerationPreset generationPreset, CancellationToken cancellationToken = default)
            => Task.FromResult(generationPreset);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(GenerationPreset generationPreset)
        {
        }
    }

    private sealed class StaticAppRuntimeDefaultsRepository(AppRuntimeDefaults defaults) : IAppRuntimeDefaultsRepository
    {
        private readonly AppRuntimeDefaults _defaults = defaults;

        public Task<AppRuntimeDefaults?> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<AppRuntimeDefaults?>(_defaults);

        public Task<AppRuntimeDefaults> GetOrCreateAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_defaults);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class EmptyRetrievalService : IRetrievalService
    {
        public Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task IndexLoreEntryAsync(Guid characterId, LoreEntry loreEntry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveSourceAsync(string sourceType, Guid sourceEntityId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<RetrievalInspectionResult> InspectAsync(Guid characterId, Guid? conversationId, string query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RetrievalInspectionResult
            {
                Query = query,
                SelectedMemories = [],
                SelectedLoreEntries = [],
                SelectedMemoryExplanations = [],
                SelectedLoreExplanations = []
            });
        }
    }

    private sealed class SimplePromptComposer : IPromptComposer
    {
        public PromptCompositionResult Compose(PromptCompositionContext context)
        {
            return new PromptCompositionResult
            {
                Prompt = "prompt-body",
                Sections =
                [
                    new PromptSection
                    {
                        Name = "History",
                        Content = "content",
                        EstimatedTokens = 5
                    }
                ],
                SelectedSceneState = [],
                SuppressedSceneState = [],
                SelectedDurableMemory = [],
                SuppressedDurableMemory = []
            };
        }
    }

    private sealed class ConstantInferenceProvider(string response) : IInferenceProvider
    {
        public async Task<string> StreamCompletionAsync(
            string prompt,
            Func<string, CancellationToken, Task> onDelta,
            InferenceExecutionSettings? executionSettings = null,
            CancellationToken cancellationToken = default)
        {
            await onDelta(response, cancellationToken);
            return response;
        }
    }

    private sealed class ConstantTokenEstimator : ITokenEstimator
    {
        public int EstimateTokens(string? text) => 1;
    }

    private sealed class StaticModelContextService : IModelContextService
    {
        public Task<ModelContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ModelContextInfo
            {
                ModelName = "test",
                EffectiveContextLength = 8192,
                ReservedOutputTokens = 1024,
                SafetyMarginTokens = 128,
                MaxPromptTokens = 7168
            });
        }

        public Task<ModelContextInfo> GetForModelAsync(string? modelIdentifier, int? contextWindowOverride, CancellationToken cancellationToken = default)
            => GetCurrentAsync(cancellationToken);
    }

    private sealed class PassthroughConversationSummaryService : IConversationSummaryService
    {
        public Task<string> BuildRollingSummaryAsync(string? existingSummary, IReadOnlyList<Message> messagesToSummarize, CancellationToken cancellationToken = default)
            => Task.FromResult(existingSummary ?? string.Empty);
    }

    private sealed class RecordingGenerationPromptSnapshotRepository : IGenerationPromptSnapshotRepository
    {
        public List<GenerationPromptSnapshot> Snapshots { get; } = [];

        public Task AddAsync(GenerationPromptSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            Snapshots.Add(snapshot);
            return Task.CompletedTask;
        }

        public Task<GenerationPromptSnapshot?> GetByMessageVariantIdAsync(Guid messageVariantId, CancellationToken cancellationToken = default)
            => Task.FromResult(Snapshots.FirstOrDefault(x => x.MessageVariantId == messageVariantId));

        public Task<IReadOnlyList<GenerationPromptSnapshot>> ListByConversationAsync(Guid conversationId, int maxCount = 100, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GenerationPromptSnapshot>>(Snapshots.Where(x => x.ConversationId == conversationId).Take(maxCount).ToList());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
