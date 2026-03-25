using LocalChat.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Seed;

public static class DefaultGenerationPresetSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.GenerationPresets.AnyAsync())
        {
            return;
        }

        var preset = new GenerationPreset
        {
            Id = Guid.NewGuid(),
            Name = "Starter Balanced Preset",
            Temperature = 0.8,
            TopP = 0.95,
            RepeatPenalty = 1.05,
            MaxOutputTokens = 1024,
            StopSequencesText = string.Empty,
            Notes = "Balanced default preset.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await dbContext.GenerationPresets.AddAsync(preset);
        await dbContext.SaveChangesAsync();
    }
}
