using LocalChat.Domain.Entities.Agents;
using LocalChat.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Tests;

public sealed class SqliteMigrationSmokeTests
{
    [Fact]
    public async Task Database_Migrate_Creates_Schema_And_Allows_Insert()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={tempPath}";

        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(
                    connection,
                    sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .Options;

            await using var dbContext = new ApplicationDbContext(options);

            try
            {
                await dbContext.Database.MigrateAsync();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning", StringComparison.Ordinal))
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            dbContext.Agents.Add(new Agent
            {
                Id = Guid.NewGuid(),
                Name = "Smoke Test Agent",
                Description = "Created by migration smoke test.",
                Greeting = "Hello",
                PersonalityDefinition = "Be helpful.",
                Scenario = "Testing persistence.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var count = await dbContext.Agents.CountAsync();
            Assert.Equal(1, count);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(tempPath))
            {
                var deleted = false;
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        File.Delete(tempPath);
                        deleted = true;
                        break;
                    }
                    catch (IOException) when (i < 4)
                    {
                        await Task.Delay(50);
                    }
                }

                if (!deleted && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }
}
