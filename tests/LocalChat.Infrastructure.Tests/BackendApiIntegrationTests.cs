using System.Net.Http.Json;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Api.Endpoints;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Features.Memory;
using LocalChat.Application.Memory;
using LocalChat.Contracts.Memory;
using LocalChat.Contracts.Models;
using LocalChat.Contracts.Personas;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Persistence;
using LocalChat.Infrastructure.Persistence.Repositories;
using LocalChat.Application.Inspection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalChat.Infrastructure.Tests;

public sealed class BackendApiIntegrationTests
{
    [Fact]
    public async Task MemoryOperations_PromoteDemoteMergeAndUndo_WorkAsExpected()
    {
        await using var harness = await TestHarness.CreateAsync();

        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Memory Character",
            Description = "Desc",
            Greeting = "Hi",
            PersonalityDefinition = "Calm",
            Scenario = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            Title = "Memory Flow",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var promotedMemory = new MemoryItem
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            ConversationId = conversation.Id,
            ScopeType = MemoryScopeType.Conversation,
            Category = MemoryCategory.UserFact,
            Kind = MemoryKind.DurableFact,
            Content = "Conversation-scoped memory",
            ReviewStatus = MemoryReviewStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var mergeTarget = new MemoryItem
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            ConversationId = conversation.Id,
            ScopeType = MemoryScopeType.Conversation,
            Category = MemoryCategory.UserFact,
            Kind = MemoryKind.DurableFact,
            Content = "Target durable fact",
            ReviewStatus = MemoryReviewStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var mergeSource = new MemoryItem
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            ConversationId = conversation.Id,
            ScopeType = MemoryScopeType.Conversation,
            Category = MemoryCategory.UserFact,
            Kind = MemoryKind.DurableFact,
            Content = "Source durable fact",
            ReviewStatus = MemoryReviewStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await harness.WithDbAsync(async db =>
        {
            db.Characters.Add(character);
            db.Conversations.Add(conversation);
            db.MemoryItems.AddRange(promotedMemory, mergeTarget, mergeSource);
            await db.SaveChangesAsync();
        });

        var promoteResponse = await harness.Client.PostAsync($"/api/memory/{promotedMemory.Id}/promote-to-character", null);
        await EnsureSuccessAsync(promoteResponse);

        await harness.WithDbAsync(async db =>
        {
            var item = await db.MemoryItems.FirstAsync(x => x.Id == promotedMemory.Id);
            Assert.Equal(MemoryScopeType.Character, item.ScopeType);
            Assert.Null(item.ConversationId);
        });

        var demoteResponse = await harness.Client.PostAsync($"/api/memory/{promotedMemory.Id}/demote-to-conversation/{conversation.Id}", null);
        await EnsureSuccessAsync(demoteResponse);

        await harness.WithDbAsync(async db =>
        {
            var item = await db.MemoryItems.FirstAsync(x => x.Id == promotedMemory.Id);
            Assert.Equal(MemoryScopeType.Conversation, item.ScopeType);
            Assert.Equal(conversation.Id, item.ConversationId);
        });

        var mergeResponse = await harness.Client.PostAsJsonAsync("/api/memory/merge", new MergeMemoryItemsRequest
        {
            SourceMemoryId = mergeSource.Id,
            TargetMemoryId = mergeTarget.Id,
            Strategy = "append_unique"
        });
        await EnsureSuccessAsync(mergeResponse);

        Guid mergeAuditId = Guid.Empty;
        await harness.WithDbAsync(async db =>
        {
            Assert.False(await db.MemoryItems.AnyAsync(x => x.Id == mergeSource.Id));

            var target = await db.MemoryItems.FirstAsync(x => x.Id == mergeTarget.Id);
            Assert.Contains("Target durable fact", target.Content);
            Assert.Contains("Source durable fact", target.Content);

            var audit = await db.MemoryOperationAudits
                .OrderByDescending(x => x.CreatedAt)
                .FirstAsync(x => x.OperationType == MemoryOperationType.MergedIntoTarget && x.TargetMemoryItemId == mergeTarget.Id);
            mergeAuditId = audit.Id;
        });

        var undoResponse = await harness.Client.PostAsync($"/api/memory/operations/{mergeAuditId}/undo", null);
        await EnsureSuccessAsync(undoResponse);

        await harness.WithDbAsync(async db =>
        {
            var restoredSource = await db.MemoryItems.FirstOrDefaultAsync(x => x.Id == mergeSource.Id);
            Assert.NotNull(restoredSource);
            Assert.Equal("Source durable fact", restoredSource!.Content);

            var revertedTarget = await db.MemoryItems.FirstAsync(x => x.Id == mergeTarget.Id);
            Assert.Equal("Target durable fact", revertedTarget.Content);
        });
    }

    [Fact]
    public async Task PersonaDefaults_FirstSetDeletePromotionAndClear_WorkAsExpected()
    {
        await using var harness = await TestHarness.CreateAsync();

        async Task<Guid> CreatePersonaAsync(string name)
        {
            var response = await harness.Client.PostAsJsonAsync("/api/personas", new CreateUserPersonaRequest
            {
                Name = name,
                DisplayName = name,
                Description = $"{name} description",
                Traits = "traits",
                Preferences = "prefs",
                AdditionalInstructions = "instructions"
            });

            await EnsureSuccessAsync(response);
            var created = await response.Content.ReadFromJsonAsync<CreatedPersona>();
            Assert.NotNull(created);
            return created!.Id;
        }

        var firstPersonaId = await CreatePersonaAsync("First");

        await harness.WithDbAsync(async db =>
        {
            var personas = await db.UserPersonas.OrderBy(x => x.CreatedAt).ToListAsync();
            Assert.Single(personas);
            Assert.True(personas[0].IsDefault);
            Assert.Equal(firstPersonaId, personas[0].Id);
        });

        var secondPersonaId = await CreatePersonaAsync("Second");

        var setDefaultResponse = await harness.Client.PostAsync($"/api/personas/{secondPersonaId}/set-default", null);
        await EnsureSuccessAsync(setDefaultResponse);

        await harness.WithDbAsync(async db =>
        {
            var personas = await db.UserPersonas.OrderBy(x => x.CreatedAt).ToListAsync();
            Assert.Equal(2, personas.Count);
            Assert.False(personas.First(x => x.Id == firstPersonaId).IsDefault);
            Assert.True(personas.First(x => x.Id == secondPersonaId).IsDefault);
        });

        var deleteDefaultResponse = await harness.Client.DeleteAsync($"/api/personas/{secondPersonaId}");
        await EnsureSuccessAsync(deleteDefaultResponse);

        await harness.WithDbAsync(async db =>
        {
            var remaining = await db.UserPersonas.ToListAsync();
            Assert.Single(remaining);
            Assert.Equal(firstPersonaId, remaining[0].Id);
            Assert.True(remaining[0].IsDefault);
        });

        var deleteLastResponse = await harness.Client.DeleteAsync($"/api/personas/{firstPersonaId}");
        await EnsureSuccessAsync(deleteLastResponse);

        await harness.WithDbAsync(async db =>
        {
            Assert.False(await db.UserPersonas.AnyAsync());
            var defaults = await db.AppRuntimeDefaults.FirstOrDefaultAsync();
            Assert.NotNull(defaults);
            Assert.Null(defaults!.DefaultPersonaId);
        });
    }

    [Fact]
    public async Task ModelProfileApi_Accepts_LlamaCpp_Provider()
    {
        await using var harness = await TestHarness.CreateAsync();

        var createResponse = await harness.Client.PostAsJsonAsync("/api/model-profiles", new CreateModelProfileRequest
        {
            Name = "Local GGUF",
            ProviderType = "llama.cpp",
            ModelIdentifier = "local-gguf-model",
            ContextWindow = 32768,
            Notes = "llama.cpp profile"
        });
        await EnsureSuccessAsync(createResponse);

        var created = await createResponse.Content.ReadFromJsonAsync<CreatedModelProfile>();
        Assert.NotNull(created);
        Assert.Equal("llama.cpp", created!.ProviderType);
        Assert.Equal("llamacpp:local-gguf-model", created.ModelIdentifier);
    }

    private sealed class CreatedPersona
    {
        public Guid Id { get; set; }
    }

    private sealed class CreatedModelProfile
    {
        public Guid Id { get; set; }

        public string ProviderType { get; set; } = string.Empty;

        public string ModelIdentifier { get; set; } = string.Empty;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new Xunit.Sdk.XunitException(
            $"Expected success but got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
    }

    private sealed class TestHarness : IAsyncDisposable
    {
        private readonly WebApplication _app;
        private readonly string _dbPath;

        private TestHarness(WebApplication app, HttpClient client, string dbPath)
        {
            _app = app;
            Client = client;
            _dbPath = dbPath;
        }

        public HttpClient Client { get; }

        public static async Task<TestHarness> CreateAsync()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"localchat-backend-tests-{Guid.NewGuid():N}.db");
            var connectionString = $"Data Source={dbPath}";

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Development"
            });

            builder.WebHost.UseTestServer();
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
            builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
            builder.Services.AddScoped<IMemoryOperationAuditService, MemoryOperationAuditService>();
            builder.Services.AddScoped<IModelProfileRepository, ModelProfileRepository>();
            builder.Services.AddScoped<IGenerationPresetRepository, GenerationPresetRepository>();
            builder.Services.AddScoped<ICharacterRepository, NoopCharacterRepository>();
            builder.Services.AddScoped<IConversationRepository, NoopConversationRepository>();
            builder.Services.AddScoped<IRetrievalService, NoopRetrievalService>();
            builder.Services.AddScoped<IMemoryProposalService, NoopMemoryProposalService>();
            builder.Services.AddScoped<IConversationRetrievalSyncService, NoopConversationRetrievalSyncService>();

            var app = builder.Build();
            app.MapMemoryEndpoints();
            app.MapMemoryAdminEndpoints();
            app.MapPersonasEndpoints();
            app.MapModelProfilesEndpoints();

            await app.StartAsync();

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    await db.Database.MigrateAsync();
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning", StringComparison.Ordinal))
                {
                    await db.Database.EnsureCreatedAsync();
                }
            }

            return new TestHarness(app, app.GetTestClient(), dbPath);
        }

        public async Task WithDbAsync(Func<ApplicationDbContext, Task> action)
        {
            await using var scope = _app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await action(db);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.DisposeAsync();
            if (File.Exists(_dbPath))
            {
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        File.Delete(_dbPath);
                        break;
                    }
                    catch (IOException) when (i < 4)
                    {
                        await Task.Delay(50);
                    }
                    catch (UnauthorizedAccessException) when (i < 4)
                    {
                        await Task.Delay(50);
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        break;
                    }
                }
            }
        }

        private sealed class NoopCharacterRepository : ICharacterRepository
        {
            public Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Character?>(null);

            public Task<Character?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Character?>(null);

            public Task<Character?> GetDefaultAsync(CancellationToken cancellationToken = default) => Task.FromResult<Character?>(null);

            public Task<IReadOnlyList<Character>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Character>>([]);

            public Task<Character> AddAsync(Character character, CancellationToken cancellationToken = default) => Task.FromResult(character);

            public Task<bool> HasConversationsAsync(Guid characterId, CancellationToken cancellationToken = default) => Task.FromResult(false);

            public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(Character character)
            {
            }
        }

        private sealed class NoopConversationRepository : IConversationRepository
        {
            public Task<Conversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Conversation?>(null);

            public Task<Conversation?> GetByMessageIdWithMessagesAsync(Guid messageId, CancellationToken cancellationToken = default) => Task.FromResult<Conversation?>(null);

            public Task<IReadOnlyList<Conversation>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Conversation>>([]);

            public Task<SummaryCheckpoint?> GetLatestSummaryAsync(Guid conversationId, CancellationToken cancellationToken = default) => Task.FromResult<SummaryCheckpoint?>(null);

            public Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default) => Task.FromResult(conversation);

            public Task AddMessageAsync(Message message, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task AddMessageVariantAsync(MessageVariant variant, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task AddSummaryCheckpointAsync(SummaryCheckpoint checkpoint, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task<int> DeleteMessagesFromSequenceAsync(Guid conversationId, int sequenceNumber, bool inclusive, CancellationToken cancellationToken = default) => Task.FromResult(0);

            public Task DeleteMessageVariantsAsync(Guid messageId, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task<int> DeleteSummaryCheckpointsAsync(Guid conversationId, CancellationToken cancellationToken = default) => Task.FromResult(0);

            public Task<int> GetNextSequenceNumberAsync(Guid conversationId, CancellationToken cancellationToken = default) => Task.FromResult(1);

            public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class NoopRetrievalService : IRetrievalService
        {
            public Task IndexMemoryAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task IndexLoreEntryAsync(Guid characterId, LoreEntry loreEntry, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task RemoveSourceAsync(string sourceType, Guid sourceEntityId, CancellationToken cancellationToken = default) => Task.CompletedTask;

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

        private sealed class NoopMemoryProposalService : IMemoryProposalService
        {
            public Task<MemoryProposalGenerationResult> GenerateForConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new MemoryProposalGenerationResult
                {
                    AttemptedCandidates = 0,
                    CreatedProposalCount = 0,
                    AutoSavedSceneStateCount = 0,
                    AutoAcceptedDurableCount = 0,
                    SceneStateReplacedCount = 0,
                    MergedDurableProposalCount = 0,
                    ConflictingDurableProposalCount = 0,
                    SkippedLowConfidenceCount = 0,
                    SkippedDuplicateCount = 0,
                    ConflictAnnotatedCount = 0,
                    InvalidCandidateCount = 0
                });
            }
        }

        private sealed class NoopConversationRetrievalSyncService : IConversationRetrievalSyncService
        {
            public Task ReindexConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}
