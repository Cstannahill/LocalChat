using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Contracts.Agents;
using LocalChat.Contracts.ImportExport;
using LocalChat.Contracts.UserProfiles;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.UserProfiles;

namespace LocalChat.Api.Endpoints;

public static class ImportExportEndpoints
{
    public static IEndpointRouteBuilder MapImportExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import-export")
            .WithTags("ImportExport");

        group.MapGet("/agents/{id:guid}", async (
            Guid id,
            IAgentRepository repository,
            CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (agent is null)
            {
                return Results.NotFound();
            }

            var model = new AgentPortableModel
            {
                Name = agent.Name,
                Description = agent.Description,
                Greeting = agent.Greeting,
                PersonalityDefinition = agent.PersonalityDefinition,
                Scenario = agent.Scenario,
                DefaultTtsVoice = agent.DefaultTtsVoice,
                DefaultVisualStylePreset = agent.DefaultVisualStylePreset,
                DefaultVisualPromptPrefix = agent.DefaultVisualPromptPrefix,
                DefaultVisualNegativePrompt = agent.DefaultVisualNegativePrompt,
                SampleDialogues = agent.SampleDialogues
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new AgentSampleDialogueRequest
                    {
                        UserMessage = x.UserMessage,
                        AssistantMessage = x.AssistantMessage
                    })
                    .ToList()
            };

            return Results.Ok(model);
        });

        group.MapPost("/agents", async (
            AgentPortableModel model,
            IAgentRepository repository,
            CancellationToken cancellationToken) =>
        {
            var agent = new Agent
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
                    .Select((x, index) => new AgentSampleDialogue
                    {
                        Id = Guid.NewGuid(),
                        UserMessage = x.UserMessage.Trim(),
                        AssistantMessage = x.AssistantMessage.Trim(),
                        SortOrder = index
                    })
                    .ToList()
            };

            await repository.AddAsync(agent, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var created = await repository.GetByIdWithDetailsAsync(agent.Id, cancellationToken);

            return Results.Ok(new AgentDetailResponse
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
                    .Select(x => new AgentSampleDialogueResponse
                    {
                        Id = x.Id,
                        UserMessage = x.UserMessage,
                        AssistantMessage = x.AssistantMessage,
                        SortOrder = x.SortOrder
                    })
                    .ToList()
            });
        });

        group.MapGet("/userProfiles/{id:guid}", async (
            Guid id,
            IUserProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            var userProfile = await repository.GetByIdAsync(id, cancellationToken);
            if (userProfile is null)
            {
                return Results.NotFound();
            }

            var model = new UserProfilePortableModel
            {
                Name = userProfile.Name,
                DisplayName = userProfile.DisplayName,
                Description = userProfile.Description,
                Traits = userProfile.Traits,
                Preferences = userProfile.Preferences,
                AdditionalInstructions = userProfile.AdditionalInstructions
            };

            return Results.Ok(model);
        });

        group.MapPost("/userProfiles", async (
            UserProfilePortableModel model,
            IUserProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            var userProfile = new UserProfile
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

            await repository.AddAsync(userProfile, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(new UserProfileResponse
            {
                Id = userProfile.Id,
                Name = userProfile.Name,
                DisplayName = userProfile.DisplayName,
                Description = userProfile.Description,
                Traits = userProfile.Traits,
                Preferences = userProfile.Preferences,
                AdditionalInstructions = userProfile.AdditionalInstructions,
                IsDefault = userProfile.IsDefault,
                CreatedAt = userProfile.CreatedAt,
                UpdatedAt = userProfile.UpdatedAt
            });
        });

        return app;
    }
}
