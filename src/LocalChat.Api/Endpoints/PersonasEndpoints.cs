using LocalChat.Contracts.Personas;
using LocalChat.Domain.Entities.Personas;
using LocalChat.Domain.Entities.Settings;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class PersonasEndpoints
{
    public static IEndpointRouteBuilder MapPersonasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/personas").WithTags("Personas");

        group.MapGet(
            "/",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var personas = await dbContext.UserPersonas
                    .AsNoTracking()
                    .OrderBy(x => x.DisplayName)
                    .ThenBy(x => x.Name)
                    .ToListAsync(cancellationToken);

                return Results.Ok(personas.Select(ToResponse).ToList());
            }
        );

        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var persona = await dbContext.UserPersonas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (persona is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(ToResponse(persona));
            }
        );

        group.MapPost(
            "/",
            async (
                CreateUserPersonaRequest request,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.BadRequest(new { error = "Persona name is required." });
                }

                var hasAnyPersonas = await dbContext.UserPersonas.AnyAsync(cancellationToken);
                var hasDefaultPersona = await dbContext.UserPersonas
                    .AnyAsync(x => x.IsDefault, cancellationToken);

                var persona = new UserPersona
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                        ? request.Name.Trim()
                        : request.DisplayName.Trim(),
                    Description = request.Description.Trim(),
                    Traits = request.Traits.Trim(),
                    Preferences = request.Preferences.Trim(),
                    AdditionalInstructions = request.AdditionalInstructions.Trim(),
                    IsDefault = !hasAnyPersonas || !hasDefaultPersona,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await dbContext.UserPersonas.AddAsync(persona, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (persona.IsDefault)
                {
                    await SyncDefaultPersonaAsync(dbContext, persona.Id, cancellationToken);
                }

                return Results.Ok(ToResponse(persona));
            }
        );

        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateUserPersonaRequest request,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var persona = await dbContext.UserPersonas
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (persona is null)
                {
                    return Results.NotFound();
                }

                persona.Name = request.Name.Trim();
                persona.DisplayName = request.DisplayName.Trim();
                persona.Description = request.Description.Trim();
                persona.Traits = request.Traits.Trim();
                persona.Preferences = request.Preferences.Trim();
                persona.AdditionalInstructions = request.AdditionalInstructions.Trim();
                persona.UpdatedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(ToResponse(persona));
            }
        );

        group.MapPost(
            "/{personaId:guid}/set-default",
            async (
                Guid personaId,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var persona = await dbContext.UserPersonas
                    .FirstOrDefaultAsync(x => x.Id == personaId, cancellationToken);
                if (persona is null)
                {
                    return Results.NotFound();
                }

                await SyncDefaultPersonaAsync(dbContext, persona.Id, cancellationToken);
                return Results.Ok();
            }
        );

        group.MapGet(
            "/{personaId:guid}/delete-preview",
            async (
                Guid personaId,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var persona = await dbContext.UserPersonas
                    .FirstOrDefaultAsync(x => x.Id == personaId, cancellationToken);
                if (persona is null)
                {
                    return Results.NotFound();
                }

                UserPersona? replacement = null;

                if (persona.IsDefault)
                {
                    replacement = await dbContext.UserPersonas
                        .Where(x => x.Id != personaId)
                        .OrderBy(x => x.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                return Results.Ok(new PersonaDeletePreviewResponse
                {
                    PersonaId = persona.Id,
                    DisplayName = string.IsNullOrWhiteSpace(persona.DisplayName)
                        ? persona.Name
                        : persona.DisplayName,
                    IsDefault = persona.IsDefault,
                    WillPromoteReplacement = persona.IsDefault && replacement is not null,
                    ReplacementPersonaId = replacement?.Id,
                    ReplacementDisplayName = replacement is null
                        ? null
                        : string.IsNullOrWhiteSpace(replacement.DisplayName)
                            ? replacement.Name
                            : replacement.DisplayName
                });
            }
        );

        group.MapDelete(
            "/{id:guid}",
            async (
                Guid id,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var persona = await dbContext.UserPersonas
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (persona is null)
                {
                    return Results.NotFound();
                }

                var wasDefault = persona.IsDefault;
                UserPersona? replacement = null;

                dbContext.UserPersonas.Remove(persona);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (wasDefault)
                {
                    replacement = await dbContext.UserPersonas
                        .OrderBy(x => x.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    await SyncDefaultPersonaAsync(
                        dbContext,
                        replacement?.Id,
                        cancellationToken);
                }

                return Results.Ok(new
                {
                    deletedPersonaId = id,
                    promotedReplacementPersonaId = replacement?.Id,
                    promotedReplacementDisplayName = replacement is null
                        ? null
                        : string.IsNullOrWhiteSpace(replacement.DisplayName)
                            ? replacement.Name
                            : replacement.DisplayName
                });
            }
        );

        return app;
    }

    private static async Task<AppRuntimeDefaults> GetOrCreateAppDefaultsAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.AppRuntimeDefaults.FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = new AppRuntimeDefaults
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await dbContext.AppRuntimeDefaults.AddAsync(created, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    private static async Task SyncDefaultPersonaAsync(
        ApplicationDbContext dbContext,
        Guid? defaultPersonaId,
        CancellationToken cancellationToken)
    {
        var personas = await dbContext.UserPersonas.ToListAsync(cancellationToken);

        foreach (var persona in personas)
        {
            persona.IsDefault = defaultPersonaId.HasValue && persona.Id == defaultPersonaId.Value;
            persona.UpdatedAt = DateTime.UtcNow;
        }

        var defaults = await GetOrCreateAppDefaultsAsync(dbContext, cancellationToken);
        defaults.DefaultPersonaId = defaultPersonaId;
        defaults.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserPersonaResponse ToResponse(UserPersona x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            DisplayName = x.DisplayName,
            Description = x.Description,
            Traits = x.Traits,
            Preferences = x.Preferences,
            AdditionalInstructions = x.AdditionalInstructions,
            IsDefault = x.IsDefault,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
        };
}
