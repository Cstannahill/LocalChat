using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Infrastructure.Persistence;
using LocalChat.Infrastructure.Retrieval.VectorStores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Tests;

public sealed class SqliteBruteForceVectorStoreTests
{
    [Fact]
    public async Task SearchAsync_RespectsSourceType_Agent_AndConversationFilters()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var dataSource = $"Data Source={dbPath};Pooling=False";

        try
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(dataSource)
                .Options;

            await using (var dbContext = new ApplicationDbContext(options))
            {
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();

                var store = new SqliteBruteForceVectorStore(dbContext);

                var agentId = Guid.NewGuid();
                var conversationId = Guid.NewGuid();

                await store.UpsertAsync(
                    new[]
                    {
                        new VectorDocument
                        {
                            SourceId = Guid.NewGuid(),
                            SourceType = "Memory",
                            AgentId = agentId,
                            ConversationId = conversationId,
                            Content = "Yellow sundress memory",
                            Embedding = new[] { 1f, 0f, 0f },
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new VectorDocument
                        {
                            SourceId = Guid.NewGuid(),
                            SourceType = "Lore",
                            AgentId = null,
                            ConversationId = null,
                            Content = "Castle balcony lore",
                            Embedding = new[] { 0f, 1f, 0f },
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new VectorDocument
                        {
                            SourceId = Guid.NewGuid(),
                            SourceType = "Memory",
                            AgentId = Guid.NewGuid(),
                            ConversationId = Guid.NewGuid(),
                            Content = "Different scope memory",
                            Embedding = new[] { 1f, 0f, 0f },
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    });

                var results = await store.SearchAsync(
                    new VectorSearchQuery
                    {
                        QueryEmbedding = new[] { 1f, 0f, 0f },
                        SourceTypes = new[] { "Memory" },
                        AgentId = agentId,
                        ConversationId = conversationId,
                        IncludeGlobalAgentItems = true,
                        IncludeGlobalConversationItems = true,
                        TopK = 10
                    });

                Assert.Single(results);
                Assert.Equal("Memory", results[0].SourceType);
                Assert.Equal(agentId, results[0].AgentId);
                Assert.Equal(conversationId, results[0].ConversationId);
                Assert.Contains("Yellow sundress", results[0].Content);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            for (var i = 0; i < 5; i++)
            {
                if (!File.Exists(dbPath))
                {
                    break;
                }

                try
                {
                    File.Delete(dbPath);
                    break;
                }
                catch (IOException) when (i < 4)
                {
                    await Task.Delay(50);
                }
            }
        }
    }
}
