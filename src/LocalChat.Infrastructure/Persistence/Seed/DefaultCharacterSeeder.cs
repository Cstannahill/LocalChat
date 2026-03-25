using LocalChat.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Seed;

public static class DefaultCharacterSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Characters.AnyAsync())
        {
            return;
        }

        var modelProfileId = await dbContext.ModelProfiles
            .OrderBy(x => x.CreatedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        var generationPresetId = await dbContext.GenerationPresets
            .OrderBy(x => x.CreatedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Starter Assistant",
            Description = "A practical, technical AI assistant for local software projects.",
            Greeting = "Hey - I'm ready. What are we building today?",
            PersonalityDefinition = """
                                    You are a practical, technically capable assistant focused on helping the user build and debug software projects.
                                    You should be direct, useful, and detail-oriented.
                                    """,
            Scenario = """
                       You are chatting with a developer inside a local-first character chat application.
                       The user is usually building software systems and wants concrete, implementation-focused help.
                       """,
            DefaultModelProfileId = modelProfileId,
            DefaultGenerationPresetId = generationPresetId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SampleDialogues =
            [
                new CharacterSampleDialogue
                {
                    Id = Guid.NewGuid(),
                    UserMessage = "I need a plan for building a feature-rich local AI chat app.",
                    AssistantMessage = "Start with a modular monolith, get the chat loop working first, then layer in memory, retrieval, and authoring tools in that order.",
                    SortOrder = 0
                }
            ]
        };

        await dbContext.Characters.AddAsync(character);
        await dbContext.SaveChangesAsync();
    }
}
