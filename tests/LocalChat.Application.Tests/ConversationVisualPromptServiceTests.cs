using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.ImageGeneration;
using LocalChat.Application.Inspection;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class ConversationVisualPromptServiceTests
{
    [Fact]
    public async Task GenerateAsync_UsesConversationContext_AndParsesCodeFencedJson()
    {
        var characterId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            CharacterId = characterId,
            Title = "Visual Prompt Test",
            SceneContext = "A quiet balcony at sunset.",
            Character = new Character
            {
                Id = characterId,
                Name = "Elena",
                Description = "A graceful woman with long silver hair.",
                Greeting = "Hello.",
                PersonalityDefinition = "Tender and poetic.",
                Scenario = "Romantic fantasy setting.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SampleDialogues = new List<CharacterSampleDialogue>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = characterId,
                        UserMessage = "How are you?",
                        AssistantMessage = "I am well.",
                        SortOrder = 0
                    }
                }
            },
            SummaryCheckpoints = new List<SummaryCheckpoint>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    StartSequenceNumber = 1,
                    EndSequenceNumber = 4,
                    SummaryText = "Elena embraced the user while wearing a yellow sundress.",
                    CreatedAt = DateTime.UtcNow
                }
            },
            Messages = new List<Message>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.User,
                    Content = "You look beautiful in that yellow sundress.",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = "*She wraps her arms around you gently.*",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow
                }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inference = new CapturingInferenceProvider(
            """
            ```json
            {
              "positivePrompt": "romantic sunset balcony embrace, woman in a yellow sundress hugging another person, soft golden light, silver hair, intimate composition",
              "negativePrompt": "blurry, low quality, bad anatomy",
              "sceneSummary": "A romantic embrace on a balcony at sunset.",
              "assumptionsOrUnknowns": ["The other person's appearance is not clearly described."]
            }
            ```
            """);

        var service = new ConversationVisualPromptService(
            new FakeConversationRepository(conversation),
            new FakeRetrievalService(),
            inference);

        var result = await service.GenerateAsync(conversationId);

        Assert.Contains("yellow sundress", result.PositivePrompt);
        Assert.Contains("balcony at sunset", result.SceneSummary!, StringComparison.OrdinalIgnoreCase);
        Assert.Single(result.AssumptionsOrUnknowns);
        Assert.Contains("Relevant Memory:", inference.LastPrompt);
        Assert.Contains("Relevant Lore:", inference.LastPrompt);
        Assert.Contains("Rolling Summary:", inference.LastPrompt);
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
            => Task.FromResult(_conversation.SummaryCheckpoints.OrderByDescending(x => x.EndSequenceNumber).FirstOrDefault());

        public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
            => Task.FromResult(conversation);

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default) => Task.CompletedTask;

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
            => Task.FromResult(1);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeRetrievalService : IRetrievalService
    {
        public Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task IndexLoreEntryAsync(
            Guid characterId,
            LoreEntry loreEntry,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveSourceAsync(
            string sourceType,
            Guid sourceEntityId,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<RetrievalInspectionResult> InspectAsync(
            Guid characterId,
            Guid? conversationId,
            string query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RetrievalInspectionResult
            {
                Query = query,
                SelectedMemories = new[]
                {
                    new MemoryItem
                    {
                        Id = Guid.NewGuid(),
                        Category = MemoryCategory.CharacterFact,
                        Content = "Elena is wearing a yellow sundress.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                },
                SelectedLoreEntries = new[]
                {
                    new LoreEntry
                    {
                        Id = Guid.NewGuid(),
                        Title = "Balcony Setting",
                        Content = "The castle balcony overlooks the sea at sunset.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                },
                SelectedMemoryExplanations = Array.Empty<SelectedMemoryExplanation>(),
                SelectedLoreExplanations = Array.Empty<SelectedLoreExplanation>()
            });
        }
    }

    private sealed class CapturingInferenceProvider : IInferenceProvider
    {
        private readonly string _response;

        public CapturingInferenceProvider(string response)
        {
            _response = response;
        }

        public string LastPrompt { get; private set; } = string.Empty;

        public Task<string> StreamCompletionAsync(
            string prompt,
            Func<string, CancellationToken, Task> onToken,
            InferenceExecutionSettings? executionSettings = null,
            CancellationToken cancellationToken = default)
        {
            LastPrompt = prompt;
            return Task.FromResult(_response);
        }
    }
}
