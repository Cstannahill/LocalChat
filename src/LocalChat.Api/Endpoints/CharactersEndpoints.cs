using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Contracts.Characters;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class CharactersEndpoints
{
    private static readonly HashSet<string> AllowedCharacterImageExtensions =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif"
    ];

    public static IEndpointRouteBuilder MapCharactersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/characters")
            .WithTags("Characters");

        group.MapGet("/", async (
            ICharacterRepository repository,
            CancellationToken cancellationToken) =>
        {
            var characters = await repository.ListAsync(cancellationToken);

            var result = characters
                .Select(ToListResponse)
                .ToList();

            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ICharacterRepository repository,
            CancellationToken cancellationToken) =>
        {
            var character = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (character is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToDetailResponse(character));
        });

        group.MapPost("/{characterId:guid}/image", async (
            Guid characterId,
            IFormFile file,
            ApplicationDbContext dbContext,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var character = await dbContext.Characters.FirstOrDefaultAsync(x => x.Id == characterId, cancellationToken);
            if (character is null)
            {
                return Results.NotFound();
            }

            if (file is null || file.Length == 0)
            {
                return Results.BadRequest("A non-empty image file is required.");
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !AllowedCharacterImageExtensions.Contains(extension))
            {
                return Results.BadRequest("Only .png, .jpg, .jpeg, .webp, and .gif files are allowed.");
            }

            if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Uploaded file must be an image.");
            }

            var root = GetCharacterImageRoot(environment);
            var characterFolder = Path.Combine(root, character.Id.ToString("N"));
            Directory.CreateDirectory(characterFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(characterFolder, fileName);

            await using (var stream = File.Create(fullPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var previousImageFullPath = string.IsNullOrWhiteSpace(character.ImagePath)
                ? null
                : Path.Combine(environment.WebRootPath, character.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            character.ImagePath = NormalizeCharacterImagePublicPath(fullPath, environment);
            character.ImageUpdatedAt = DateTime.UtcNow;
            character.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            TryDeleteFile(previousImageFullPath);

            return Results.Ok(new
            {
                characterId = character.Id,
                imageUrl = ToCharacterImageUrl(character.ImagePath),
                imageUpdatedAt = character.ImageUpdatedAt
            });
        });

        group.MapPost("/", async (
            CreateCharacterRequest request,
            ICharacterRepository repository,
            IModelProfileRepository modelProfileRepository,
            IGenerationPresetRepository generationPresetRepository,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { error = "Character name is required." });
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

            var character = new Character
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
            return Results.Ok(ToDetailResponse(created!));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCharacterRequest request,
            ICharacterRepository repository,
            IModelProfileRepository modelProfileRepository,
            IGenerationPresetRepository generationPresetRepository,
            CancellationToken cancellationToken) =>
        {
            var character = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (character is null)
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

            character.Name = request.Name.Trim();
            character.Description = request.Description.Trim();
            character.Greeting = request.Greeting.Trim();
            character.PersonalityDefinition = request.PersonalityDefinition.Trim();
            character.Scenario = request.Scenario.Trim();
            character.DefaultModelProfileId = request.DefaultModelProfileId;
            character.DefaultGenerationPresetId = request.DefaultGenerationPresetId;
            character.DefaultTtsVoice = string.IsNullOrWhiteSpace(request.DefaultTtsVoice) ? null : request.DefaultTtsVoice.Trim();
            character.DefaultVisualStylePreset = string.IsNullOrWhiteSpace(request.DefaultVisualStylePreset) ? null : request.DefaultVisualStylePreset.Trim();
            character.DefaultVisualPromptPrefix = string.IsNullOrWhiteSpace(request.DefaultVisualPromptPrefix) ? null : request.DefaultVisualPromptPrefix.Trim();
            character.DefaultVisualNegativePrompt = string.IsNullOrWhiteSpace(request.DefaultVisualNegativePrompt) ? null : request.DefaultVisualNegativePrompt.Trim();
            character.UpdatedAt = DateTime.UtcNow;

            character.SampleDialogues.Clear();

            foreach (var dialogue in request.SampleDialogues
                         .Where(x =>
                             !string.IsNullOrWhiteSpace(x.UserMessage) &&
                             !string.IsNullOrWhiteSpace(x.AssistantMessage))
                         .Select((x, index) => new CharacterSampleDialogue
                         {
                             Id = Guid.NewGuid(),
                             CharacterId = character.Id,
                             UserMessage = x.UserMessage.Trim(),
                             AssistantMessage = x.AssistantMessage.Trim(),
                             SortOrder = index
                         }))
            {
                character.SampleDialogues.Add(dialogue);
            }

            await repository.SaveChangesAsync(cancellationToken);

            var updated = await repository.GetByIdWithDetailsAsync(character.Id, cancellationToken);
            return Results.Ok(ToDetailResponse(updated!));
        });

        group.MapDelete("/{characterId:guid}/image", async (
            Guid characterId,
            ApplicationDbContext dbContext,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var character = await dbContext.Characters.FirstOrDefaultAsync(x => x.Id == characterId, cancellationToken);
            if (character is null)
            {
                return Results.NotFound();
            }

            var previousImageFullPath = string.IsNullOrWhiteSpace(character.ImagePath)
                ? null
                : Path.Combine(environment.WebRootPath, character.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            character.ImagePath = null;
            character.ImageUpdatedAt = DateTime.UtcNow;
            character.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            TryDeleteFile(previousImageFullPath);

            return Results.Ok(new
            {
                characterId = character.Id
            });
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICharacterRepository repository,
            CancellationToken cancellationToken) =>
        {
            var character = await repository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (character is null)
            {
                return Results.NotFound();
            }

            var hasConversations = await repository.HasConversationsAsync(id, cancellationToken);
            if (hasConversations)
            {
                return Results.BadRequest(new { error = "Cannot delete a character that already has conversations." });
            }

            repository.Remove(character);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });

        return app;
    }

    private static CharacterResponse ToListResponse(Character x) =>
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
            ImageUrl = ToCharacterImageUrl(x.ImagePath),
            ImageUpdatedAt = x.ImageUpdatedAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    private static CharacterDetailResponse ToDetailResponse(Character x) =>
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
            ImageUrl = ToCharacterImageUrl(x.ImagePath),
            ImageUpdatedAt = x.ImageUpdatedAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            SampleDialogues = x.SampleDialogues
                .OrderBy(d => d.SortOrder)
                .Select(d => new CharacterSampleDialogueResponse
                {
                    Id = d.Id,
                    UserMessage = d.UserMessage,
                    AssistantMessage = d.AssistantMessage,
                    SortOrder = d.SortOrder
                })
                .ToList()
        };

    private static string GetCharacterImageRoot(IWebHostEnvironment environment)
    {
        return Path.Combine(environment.WebRootPath, "uploads", "characters");
    }

    private static string? NormalizeCharacterImagePublicPath(string? fullPath, IWebHostEnvironment environment)
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

    private static string? ToCharacterImageUrl(string? imagePath)
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
