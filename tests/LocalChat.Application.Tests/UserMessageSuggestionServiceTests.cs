using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Chat;
using LocalChat.Application.Inspection;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class UserMessageSuggestionServiceTests
{
    [Fact]
    public async Task GenerateAsync_ParsesWrappedJson_AndStripsUserPrefix()
    {
        var characterId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = new Conversation
        {
            Id = conversationId,
            CharacterId = characterId,
            Title = "Suggestion Test",
            Character = new Character
            {
                Id = characterId,
                Name = "Cassandra",
                Description = "A charismatic woman at a bar.",
                Greeting = "Hello.",
                PersonalityDefinition = "Intense and intimate.",
                Scenario = "A late-night bar conversation.",
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
                    Content = "What do you think alcohol really does to people?",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = "\"It strips away the armor.\"",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow
                }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = new UserMessageSuggestionService(
            new FakeConversationRepository(conversation),
            new FakeRetrievalService(),
            new FakeInferenceProvider(
                """
                Here is your result:

                ```json
                {
                  "suggestedMessage": "User: Then maybe I am more afraid of what I will say than what I will drink.",
                  "tone": "vulnerable, reflective",
                  "reasoningSummary": "The assistant framed alcohol as truth-revealing, so a vulnerable response fits naturally."
                }
                ```
                """
            )
        );

        var result = await service.GenerateAsync(conversationId);

        Assert.Equal(
            "Then maybe I am more afraid of what I will say than what I will drink.",
            result.SuggestedMessage
        );
        Assert.Equal("vulnerable, reflective", result.Tone);
        Assert.Contains("truth-revealing", result.ReasoningSummary);
    }

    [Fact]
    public async Task GenerateAsync_UsesFallback_WhenModelReturnsEmptyOutput()
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
                Name = "Cassandra",
                Description = "A charismatic woman at a bar.",
                Greeting = "Hello.",
                PersonalityDefinition = "Intense and intimate.",
                Scenario = "A late-night bar conversation.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            Messages =
            [
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.User,
                    Content = "Tell me what you mean.",
                    SequenceNumber = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Role = MessageRole.Assistant,
                    Content = "I mean you are hiding from the truth.",
                    SequenceNumber = 2,
                    CreatedAt = DateTime.UtcNow
                }
            ],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = new UserMessageSuggestionService(
            new FakeConversationRepository(conversation),
            new FakeRetrievalService(),
            new FakeInferenceProvider(string.Empty)
        );

        var result = await service.GenerateAsync(conversationId);

        Assert.Equal("Can you tell me more about that?", result.SuggestedMessage);
        Assert.NotNull(result.ReasoningSummary);
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Conversation _conversation;

        public FakeConversationRepository(Conversation conversation)
        {
            _conversation = conversation;
        }

        public Task<Conversation?> GetByIdWithMessagesAsync(
            Guid id,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(id == _conversation.Id ? _conversation : null);

        public Task<Conversation?> GetByMessageIdWithMessagesAsync(
            Guid messageId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<Conversation?>(null);

        public Task<IReadOnlyList<Conversation>> ListByCharacterAsync(
            Guid characterId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyList<Conversation>>(new[] { _conversation });

        public Task<SummaryCheckpoint?> GetLatestSummaryAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<SummaryCheckpoint?>(null);

        public Task<Conversation> AddAsync(
            Conversation conversation,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(conversation);

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AddMessageVariantAsync(
            MessageVariant variant,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task AddSummaryCheckpointAsync(
            SummaryCheckpoint checkpoint,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task<int> DeleteMessagesFromSequenceAsync(
            Guid conversationId,
            int sequenceNumber,
            bool inclusive,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(0);

        public Task DeleteMessageVariantsAsync(
            Guid messageId,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task<int> DeleteSummaryCheckpointsAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(0);

        public Task<int> GetNextSequenceNumberAsync(
            Guid conversationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(1);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeRetrievalService : IRetrievalService
    {
        public Task IndexMemoryAsync(
            MemoryItem memoryItem,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task IndexLoreEntryAsync(
            Guid characterId,
            LoreEntry loreEntry,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task RemoveSourceAsync(
            string sourceType,
            Guid sourceEntityId,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task<RetrievalInspectionResult> InspectAsync(
            Guid characterId,
            Guid? conversationId,
            string query,
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(
                new RetrievalInspectionResult
                {
                    Query = query,
                    SelectedMemories =
                    [
                        new MemoryItem
                        {
                            Id = Guid.NewGuid(),
                            Category = MemoryCategory.RelationshipFact,
                            Content =
                                "The user tends to open up when conversations get emotionally honest.",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    ],
                    SelectedLoreEntries = Array.Empty<LoreEntry>(),
                    SelectedMemoryExplanations = Array.Empty<SelectedMemoryExplanation>(),
                    SelectedLoreExplanations = Array.Empty<SelectedLoreExplanation>()
                }
            );
        }
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
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(_response);
        }
    }
}
