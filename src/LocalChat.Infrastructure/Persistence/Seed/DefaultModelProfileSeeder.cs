using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Seed;

public static class DefaultModelProfileSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.ModelProfiles.AnyAsync())
        {
            return;
        }

        var profile = new ModelProfile
        {
            Id = Guid.NewGuid(),
            Name = "Starter Ollama Profile",
            ProviderType = ProviderType.Ollama,
            ModelIdentifier = "Qwen35-2B-GPT",
            ContextWindow = null,
            Notes = "Default starter model profile.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await dbContext.ModelProfiles.AddAsync(profile);
        await dbContext.SaveChangesAsync();
    }
}
