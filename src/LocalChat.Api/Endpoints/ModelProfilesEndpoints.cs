using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Contracts.Models;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Enums;

namespace LocalChat.Api.Endpoints;

public static class ModelProfilesEndpoints
{
    public static IEndpointRouteBuilder MapModelProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        var profilesGroup = app.MapGroup("/api/model-profiles")
            .WithTags("ModelProfiles");

        profilesGroup.MapGet("/", async (
            IModelProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            var items = await repository.ListAsync(cancellationToken);
            return Results.Ok(items.Select(ToResponse).ToList());
        });

        profilesGroup.MapGet("/{id:guid}", async (
            Guid id,
            IModelProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = await repository.GetByIdAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(ToResponse(item));
        });

        profilesGroup.MapPost("/", async (
            CreateModelProfileRequest request,
            IModelProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            if (!ModelRoute.TryParseProvider(request.ProviderType, out var providerType))
            {
                return Results.BadRequest("Provider must be one of: 'ollama', 'openrouter', 'huggingface'/'hf', or 'llama.cpp'/'llamacpp'.");
            }

            var normalizedModelIdentifier = ModelRoute.NormalizeForStorage(
                providerType,
                request.ModelIdentifier);

            if ((providerType == ProviderType.OpenRouter || providerType == ProviderType.HuggingFace) &&
                string.IsNullOrWhiteSpace(normalizedModelIdentifier))
            {
                return Results.BadRequest("External provider model identifier is required.");
            }

            var item = new ModelProfile
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                ProviderType = providerType,
                ModelIdentifier = normalizedModelIdentifier,
                ContextWindow = request.ContextWindow,
                Notes = request.Notes.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(item, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(item));
        });

        profilesGroup.MapPut("/{id:guid}", async (
            Guid id,
            UpdateModelProfileRequest request,
            IModelProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = await repository.GetByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return Results.NotFound();
            }

            if (!ModelRoute.TryParseProvider(request.ProviderType, out var providerType))
            {
                return Results.BadRequest("Provider must be one of: 'ollama', 'openrouter', 'huggingface'/'hf', or 'llama.cpp'/'llamacpp'.");
            }

            var normalizedModelIdentifier = ModelRoute.NormalizeForStorage(
                providerType,
                request.ModelIdentifier);

            if ((providerType == ProviderType.OpenRouter || providerType == ProviderType.HuggingFace) &&
                string.IsNullOrWhiteSpace(normalizedModelIdentifier))
            {
                return Results.BadRequest("External provider model identifier is required.");
            }

            item.Name = request.Name.Trim();
            item.ProviderType = providerType;
            item.ModelIdentifier = normalizedModelIdentifier;
            item.ContextWindow = request.ContextWindow;
            item.Notes = request.Notes.Trim();
            item.UpdatedAt = DateTime.UtcNow;

            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(item));
        });

        profilesGroup.MapDelete("/{id:guid}", async (
            Guid id,
            IModelProfileRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = await repository.GetByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return Results.NotFound();
            }

            repository.Remove(item);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });

        var presetsGroup = app.MapGroup("/api/generation-presets")
            .WithTags("GenerationPresets");

        presetsGroup.MapGet("/", async (
            IGenerationPresetRepository repository,
            CancellationToken cancellationToken) =>
        {
            var items = await repository.ListAsync(cancellationToken);
            return Results.Ok(items.Select(ToResponse).ToList());
        });

        presetsGroup.MapGet("/{id:guid}", async (
            Guid id,
            IGenerationPresetRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = await repository.GetByIdAsync(id, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(ToResponse(item));
        });

        presetsGroup.MapPost("/", async (
            CreateGenerationPresetRequest request,
            IGenerationPresetRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = new GenerationPreset
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Temperature = request.Temperature,
                TopP = request.TopP,
                RepeatPenalty = request.RepeatPenalty,
                MaxOutputTokens = request.MaxOutputTokens,
                StopSequencesText = request.StopSequencesText.Trim(),
                Notes = request.Notes.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(item, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(item));
        });

        presetsGroup.MapPut("/{id:guid}", async (
            Guid id,
            UpdateGenerationPresetRequest request,
            IGenerationPresetRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = await repository.GetByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return Results.NotFound();
            }

            item.Name = request.Name.Trim();
            item.Temperature = request.Temperature;
            item.TopP = request.TopP;
            item.RepeatPenalty = request.RepeatPenalty;
            item.MaxOutputTokens = request.MaxOutputTokens;
            item.StopSequencesText = request.StopSequencesText.Trim();
            item.Notes = request.Notes.Trim();
            item.UpdatedAt = DateTime.UtcNow;

            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(item));
        });

        presetsGroup.MapDelete("/{id:guid}", async (
            Guid id,
            IGenerationPresetRepository repository,
            CancellationToken cancellationToken) =>
        {
            var item = await repository.GetByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return Results.NotFound();
            }

            repository.Remove(item);
            await repository.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        });

        return app;
    }

    private static ModelProfileResponse ToResponse(ModelProfile x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            ProviderType = ModelRoute.ProviderToWireValue(x.ProviderType),
            ModelIdentifier = x.ModelIdentifier,
            ContextWindow = x.ContextWindow,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    private static GenerationPresetResponse ToResponse(GenerationPreset x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            Temperature = x.Temperature,
            TopP = x.TopP,
            RepeatPenalty = x.RepeatPenalty,
            MaxOutputTokens = x.MaxOutputTokens,
            StopSequencesText = x.StopSequencesText,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
}
