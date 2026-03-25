using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Contracts.Agents;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class AgentsEndpoints
{
    private static readonly HashSet<string> AllowedAgentImageExtensions =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif"
    ];

    public static IEndpointRouteBuilder MapAgentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agents")
            .WithTags("Agents");

        group.MapGet("/", async (
            IAgentRepository repository,
            CancellationToken cancellationToken) =>
        {
            var agents = await repository.ListAsync(cancellationToken);

            var result = agents
                .Select(ToListResponse)
                .ToList();

            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IAgentRepository repository,
            CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (agent is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToDetailResponse(agent));
        });

        group.MapPost("/{agentId:guid}/image", async (
            Guid agentId,
            IFormFile file,
            ApplicationDbContext dbContext,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var agent = await dbContext.Agents.FirstOrDefaultAsync(x => x.Id == agentId, cancellationToken);
            if (agent is null)
            {
                return Results.NotFound();
            }

            if (file is null || file.Length == 0)
            {
                return Results.BadRequest("A non-empty image file is required.");
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !AllowedAgentImageExtensions.Contains(extension))
            {
                return Results.BadRequest("Only .png, .jpg, .jpeg, .webp, and .gif files are allowed.");
            }

            if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Uploaded file must be an image.");
            }

            var root = GetAgentImageRoot(environment);
            var agentFolder = Path.Combine(root, agent.Id.ToString("N"));
            Directory.CreateDirectory(agentFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(agentFolder, fileName);

            await using (var stream = File.Create(fullPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var previousImageFullPath = string.IsNullOrWhiteSpace(agent.ImagePath)
                ? null
                : Path.Combine(environment.WebRootPath, agent.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            agent.ImagePath = NormalizeAgentImagePublicPath(fullPath, environment);
            agent.ImageUpdatedAt = DateTime.UtcNow;
            agent.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            TryDeleteFile(previousImageFullPath);

            return Results.Ok(new
            {
                agentId = agent.Id,
                imageUrl = ToAgentImageUrl(agent.ImagePath),
                imageUpdatedAt = agent.ImageUpdatedAt
            });
        });

        group.MapPost("/", async (
            CreateAgentRequest request,
            IAgentRepository repository,
            IModelProfileRepository modelProfileRepository,
            IGenerationPresetRepository generationPresetRepository,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { error = "Agent name is required." });
            }

            if (request.DefaultModelProfileId.HasValue)
            {
                var modelProfile = await modelProfileRepository.GetByIdAsync(request.DefaultModelProfileId.Value, cancellationToken);
                if (modelProfile is null)
                {
                    return Results.BadRequest(new { error = $"Model profile '{request.DefaultModelProfileId.Value}' was not found." });
                }
            }

            if (request.DefaultGenerationPresetId.HasValue)
            {
                var generationPreset = await generationPresetRepository.GetByIdAsync(request.DefaultGenerationPresetId.Value, cancellationToken);
                if (generationPreset is null)
                {
                    return Results.BadRequest(new { error = $"Generation preset '{request.DefaultGenerationPresetId.Value}' was not found." });
                }
            }

            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Greeting = request.Greeting.Trim(),
                PersonalityDefinition = request.PersonalityDefinition.Trim(),
                Scenario = request.Scenario.Trim(),
                DefaultModelProfileId = request.DefaultModelProfileId,
                DefaultGenerationPresetId = request.DefaultGenerationPresetId,
                DefaultTtsVoice = string.IsNullOrWhiteSpace(request.DefaultTtsVoice) ? null : request.DefaultTtsVoice.Trim(),
                DefaultVisualStylePreset = string.IsNullOrWhiteSpace(request.DefaultVisualStylePreset) ? null : request.DefaultVisualStylePreset.Trim(),
                DefaultVisualPromptPrefix = string.IsNullOrWhiteSpace(request.DefaultVisualPromptPrefix) ? null : request.DefaultVisualPromptPrefix.Trim(),
                DefaultVisualNegativePrompt = string.IsNullOrWhiteSpace(request.DefaultVisualNegativePrompt) ? null : request.DefaultVisualNegativePrompt.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SampleDialogues = request.SampleDialogues
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
            return Results.Ok(ToDetailResponse(created!));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateAgentRequest request,
            IAgentRepository repository,
            IModelProfileRepository modelProfileRepository,
            IGenerationPresetRepository generationPresetRepository,
            CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (agent is null)
            {
                return Results.NotFound();
            }

            if (request.DefaultModelProfileId.HasValue)
            {
                var modelProfile = await modelProfileRepository.GetByIdAsync(request.DefaultModelProfileId.Value, cancellationToken);
                if (modelProfile is null)
                {
                    return Results.BadRequest(new { error = $"Model profile '{request.DefaultModelProfileId.Value}' was not found." });
                }
            }

            if (request.DefaultGenerationPresetId.HasValue)
            {
                var generationPreset = await generationPresetRepository.GetByIdAsync(request.DefaultGenerationPresetId.Value, cancellationToken);
                if (generationPreset is null)
                {
                    return Results.BadRequest(new { error = $"Generation preset '{request.DefaultGenerationPresetId.Value}' was not found." });
                }
            }

            agent.Name = request.Name.Trim();
            agent.Description = request.Description.Trim();
            agent.Greeting = request.Greeting.Trim();
            agent.PersonalityDefinition = request.PersonalityDefinition.Trim();
            agent.Scenario = request.Scenario.Trim();
            agent.DefaultModelProfileId = request.DefaultModelProfileId;
            agent.DefaultGenerationPresetId = request.DefaultGenerationPresetId;
            agent.DefaultTtsVoice = string.IsNullOrWhiteSpace(request.DefaultTtsVoice) ? null : request.DefaultTtsVoice.Trim();
            agent.DefaultVisualStylePreset = string.IsNullOrWhiteSpace(request.DefaultVisualStylePreset) ? null : request.DefaultVisualStylePreset.Trim();
            agent.DefaultVisualPromptPrefix = string.IsNullOrWhiteSpace(request.DefaultVisualPromptPrefix) ? null : request.DefaultVisualPromptPrefix.Trim();
            agent.DefaultVisualNegativePrompt = string.IsNullOrWhiteSpace(request.DefaultVisualNegativePrompt) ? null : request.DefaultVisualNegativePrompt.Trim();
            agent.UpdatedAt = DateTime.UtcNow;

            agent.SampleDialogues.Clear();

            foreach (var dialogue in request.SampleDialogues
                         .Where(x =>
                             !string.IsNullOrWhiteSpace(x.UserMessage) &&
                             !string.IsNullOrWhiteSpace(x.AssistantMessage))
                         .Select((x, index) => new AgentSampleDialogue
                         {
                             Id = Guid.NewGuid(),
                             AgentId = agent.Id,
                             UserMessage = x.UserMessage.Trim(),
                             AssistantMessage = x.AssistantMessage.Trim(),
                             SortOrder = index
                         }))
            {
                agent.SampleDialogues.Add(dialogue);
            }

            await repository.SaveChangesAsync(cancellationToken);

            var updated = await repository.GetByIdWithDetailsAsync(agent.Id, cancellationToken);
            return Results.Ok(ToDetailResponse(updated!));
        });

        group.MapDelete("/{agentId:guid}/image", async (
            Guid agentId,
            ApplicationDbContext dbContext,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var agent = await dbContext.Agents.FirstOrDefaultAsync(x => x.Id == agentId, cancellationToken);
            if (agent is null)
            {
                return Results.NotFound();
            }

            var previousImageFullPath = string.IsNullOrWhiteSpace(agent.ImagePath)
                ? null
                : Path.Combine(environment.WebRootPath, agent.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            agent.ImagePath = null;
            agent.ImageUpdatedAt = DateTime.UtcNow;
            agent.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            TryDeleteFile(previousImageFullPath);

            return Results.Ok(new
            {
                agentId = agent.Id
            });
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IAgentRepository repository,
            CancellationToken cancellationToken) =>
        {
            var agent = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (agent is null)
            {
                return Results.NotFound();
            }

            var hasConversations = await repository.HasConversationsAsync(id, cancellationToken);
            if (hasConversations)
            {
                return Results.BadRequest(new { error = "Cannot delete a agent that already has conversations." });
            }

            repository.Remove(agent);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });

        return app;
    }

    private static AgentResponse ToListResponse(Agent x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Greeting = x.Greeting,
            DefaultModelProfileId = x.DefaultModelProfileId,
            DefaultGenerationPresetId = x.DefaultGenerationPresetId,
            DefaultTtsVoice = x.DefaultTtsVoice,
            DefaultVisualStylePreset = x.DefaultVisualStylePreset,
            DefaultVisualPromptPrefix = x.DefaultVisualPromptPrefix,
            DefaultVisualNegativePrompt = x.DefaultVisualNegativePrompt,
            ImageUrl = ToAgentImageUrl(x.ImagePath),
            ImageUpdatedAt = x.ImageUpdatedAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    private static AgentDetailResponse ToDetailResponse(Agent x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Greeting = x.Greeting,
            PersonalityDefinition = x.PersonalityDefinition,
            Scenario = x.Scenario,
            DefaultModelProfileId = x.DefaultModelProfileId,
            DefaultGenerationPresetId = x.DefaultGenerationPresetId,
            DefaultTtsVoice = x.DefaultTtsVoice,
            DefaultVisualStylePreset = x.DefaultVisualStylePreset,
            DefaultVisualPromptPrefix = x.DefaultVisualPromptPrefix,
            DefaultVisualNegativePrompt = x.DefaultVisualNegativePrompt,
            ImageUrl = ToAgentImageUrl(x.ImagePath),
            ImageUpdatedAt = x.ImageUpdatedAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            SampleDialogues = x.SampleDialogues
                .OrderBy(d => d.SortOrder)
                .Select(d => new AgentSampleDialogueResponse
                {
                    Id = d.Id,
                    UserMessage = d.UserMessage,
                    AssistantMessage = d.AssistantMessage,
                    SortOrder = d.SortOrder
                })
                .ToList()
        };

    private static string GetAgentImageRoot(IWebHostEnvironment environment)
    {
        return Path.Combine(environment.WebRootPath, "uploads", "agents");
    }

    private static string? NormalizeAgentImagePublicPath(string? fullPath, IWebHostEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return null;
        }

        var webRoot = environment.WebRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalized = fullPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        if (!normalized.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath.Replace("\\", "/");
        }

        var relative = normalized[webRoot.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return "/" + relative.Replace("\\", "/");
    }

    private static string? ToAgentImageUrl(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        return imagePath.Replace("\\", "/");
    }

    private static void TryDeleteFile(string? fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
