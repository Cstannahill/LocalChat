using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Contracts.Characters;
using LocalChat.Contracts.ImportExport;
using LocalChat.Contracts.Personas;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Personas;

namespace LocalChat.Api.Endpoints;

public static class ImportExportEndpoints
{
    public static IEndpointRouteBuilder MapImportExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import-export")
            .WithTags("ImportExport");

        group.MapGet("/characters/{id:guid}", async (
            Guid id,
            ICharacterRepository repository,
            CancellationToken cancellationToken) =>
        {
            var character = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (character is null)
            {
                return Results.NotFound();
            }

            var model = new CharacterPortableModel
            {
                Name = character.Name,
                Description = character.Description,
                Greeting = character.Greeting,
                PersonalityDefinition = character.PersonalityDefinition,
                Scenario = character.Scenario,
                DefaultTtsVoice = character.DefaultTtsVoice,
                DefaultVisualStylePreset = character.DefaultVisualStylePreset,
                DefaultVisualPromptPrefix = character.DefaultVisualPromptPrefix,
                DefaultVisualNegativePrompt = character.DefaultVisualNegativePrompt,
                SampleDialogues = character.SampleDialogues
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new CharacterSampleDialogueRequest
                    {
                        UserMessage = x.UserMessage,
                        AssistantMessage = x.AssistantMessage
                    })
                    .ToList()
            };

            return Results.Ok(model);
        });

        group.MapPost("/characters", async (
            CharacterPortableModel model,
            ICharacterRepository repository,
            CancellationToken cancellationToken) =>
        {
            var character = new Character
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Greeting = model.Greeting.Trim(),
                PersonalityDefinition = model.PersonalityDefinition.Trim(),
                Scenario = model.Scenario.Trim(),
                DefaultTtsVoice = string.IsNullOrWhiteSpace(model.DefaultTtsVoice) ? null : model.DefaultTtsVoice.Trim(),
                DefaultVisualStylePreset = string.IsNullOrWhiteSpace(model.DefaultVisualStylePreset) ? null : model.DefaultVisualStylePreset.Trim(),
                DefaultVisualPromptPrefix = string.IsNullOrWhiteSpace(model.DefaultVisualPromptPrefix) ? null : model.DefaultVisualPromptPrefix.Trim(),
                DefaultVisualNegativePrompt = string.IsNullOrWhiteSpace(model.DefaultVisualNegativePrompt) ? null : model.DefaultVisualNegativePrompt.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SampleDialogues = model.SampleDialogues
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.UserMessage) &&
                        !string.IsNullOrWhiteSpace(x.AssistantMessage))
                    .Select((x, index) => new CharacterSampleDialogue
                    {
                        Id = Guid.NewGuid(),
                        UserMessage = x.UserMessage.Trim(),
                        AssistantMessage = x.AssistantMessage.Trim(),
                        SortOrder = index
                    })
                    .ToList()
            };

            await repository.AddAsync(character, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var created = await repository.GetByIdWithDetailsAsync(character.Id, cancellationToken);

            return Results.Ok(new CharacterDetailResponse
            {
                Id = created!.Id,
                Name = created.Name,
                Description = created.Description,
                Greeting = created.Greeting,
                PersonalityDefinition = created.PersonalityDefinition,
                Scenario = created.Scenario,
                DefaultModelProfileId = created.DefaultModelProfileId,
                DefaultGenerationPresetId = created.DefaultGenerationPresetId,
                DefaultTtsVoice = created.DefaultTtsVoice,
                DefaultVisualStylePreset = created.DefaultVisualStylePreset,
                DefaultVisualPromptPrefix = created.DefaultVisualPromptPrefix,
                DefaultVisualNegativePrompt = created.DefaultVisualNegativePrompt,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                SampleDialogues = created.SampleDialogues
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new CharacterSampleDialogueResponse
                    {
                        Id = x.Id,
                        UserMessage = x.UserMessage,
                        AssistantMessage = x.AssistantMessage,
                        SortOrder = x.SortOrder
                    })
                    .ToList()
            });
        });

        group.MapGet("/personas/{id:guid}", async (
            Guid id,
            IUserPersonaRepository repository,
            CancellationToken cancellationToken) =>
        {
            var persona = await repository.GetByIdAsync(id, cancellationToken);
            if (persona is null)
            {
                return Results.NotFound();
            }

            var model = new PersonaPortableModel
            {
                Name = persona.Name,
                DisplayName = persona.DisplayName,
                Description = persona.Description,
                Traits = persona.Traits,
                Preferences = persona.Preferences,
                AdditionalInstructions = persona.AdditionalInstructions
            };

            return Results.Ok(model);
        });

        group.MapPost("/personas", async (
            PersonaPortableModel model,
            IUserPersonaRepository repository,
            CancellationToken cancellationToken) =>
        {
            var persona = new UserPersona
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                DisplayName = model.DisplayName.Trim(),
                Description = model.Description.Trim(),
                Traits = model.Traits.Trim(),
                Preferences = model.Preferences.Trim(),
                AdditionalInstructions = model.AdditionalInstructions.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(persona, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(new UserPersonaResponse
            {
                Id = persona.Id,
                Name = persona.Name,
                DisplayName = persona.DisplayName,
                Description = persona.Description,
                Traits = persona.Traits,
                Preferences = persona.Preferences,
                AdditionalInstructions = persona.AdditionalInstructions,
                IsDefault = persona.IsDefault,
                CreatedAt = persona.CreatedAt,
                UpdatedAt = persona.UpdatedAt
            });
        });

        return app;
    }
}
