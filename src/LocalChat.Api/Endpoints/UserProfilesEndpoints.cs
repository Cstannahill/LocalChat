using LocalChat.Contracts.UserProfiles;
using LocalChat.Domain.Entities.UserProfiles;
using LocalChat.Domain.Entities.Settings;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class UserProfilesEndpoints
{
    public static IEndpointRouteBuilder MapUserProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user-profiles").WithTags("UserProfiles");

        group.MapGet(
            "/",
            async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
            {
                var userProfiles = await dbContext.UserProfiles
                    .AsNoTracking()
                    .OrderBy(x => x.DisplayName)
                    .ThenBy(x => x.Name)
                    .ToListAsync(cancellationToken);

                return Results.Ok(userProfiles.Select(ToResponse).ToList());
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
                var userProfile = await dbContext.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (userProfile is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(ToResponse(userProfile));
            }
        );

        group.MapPost(
            "/",
            async (
                CreateUserProfileRequest request,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return Results.BadRequest(new { error = "UserProfile name is required." });
                }

                var hasAnyUserProfiles = await dbContext.UserProfiles.AnyAsync(cancellationToken);
                var hasDefaultUserProfile = await dbContext.UserProfiles
                    .AnyAsync(x => x.IsDefault, cancellationToken);

                var userProfile = new UserProfile
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
                    IsDefault = !hasAnyUserProfiles || !hasDefaultUserProfile,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await dbContext.UserProfiles.AddAsync(userProfile, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (userProfile.IsDefault)
                {
                    await SyncDefaultUserProfileAsync(dbContext, userProfile.Id, cancellationToken);
                }

                return Results.Ok(ToResponse(userProfile));
            }
        );

        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateUserProfileRequest request,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var userProfile = await dbContext.UserProfiles
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (userProfile is null)
                {
                    return Results.NotFound();
                }

                userProfile.Name = request.Name.Trim();
                userProfile.DisplayName = request.DisplayName.Trim();
                userProfile.Description = request.Description.Trim();
                userProfile.Traits = request.Traits.Trim();
                userProfile.Preferences = request.Preferences.Trim();
                userProfile.AdditionalInstructions = request.AdditionalInstructions.Trim();
                userProfile.UpdatedAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok(ToResponse(userProfile));
            }
        );

        group.MapPost(
            "/{userProfileId:guid}/set-default",
            async (
                Guid userProfileId,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var userProfile = await dbContext.UserProfiles
                    .FirstOrDefaultAsync(x => x.Id == userProfileId, cancellationToken);
                if (userProfile is null)
                {
                    return Results.NotFound();
                }

                await SyncDefaultUserProfileAsync(dbContext, userProfile.Id, cancellationToken);
                return Results.Ok();
            }
        );

        group.MapGet(
            "/{userProfileId:guid}/delete-preview",
            async (
                Guid userProfileId,
                ApplicationDbContext dbContext,
                CancellationToken cancellationToken
            ) =>
            {
                var userProfile = await dbContext.UserProfiles
                    .FirstOrDefaultAsync(x => x.Id == userProfileId, cancellationToken);
                if (userProfile is null)
                {
                    return Results.NotFound();
                }

                UserProfile? replacement = null;

                if (userProfile.IsDefault)
                {
                    replacement = await dbContext.UserProfiles
                        .Where(x => x.Id != userProfileId)
                        .OrderBy(x => x.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                return Results.Ok(new UserProfileDeletePreviewResponse
                {
                    UserProfileId = userProfile.Id,
                    DisplayName = string.IsNullOrWhiteSpace(userProfile.DisplayName)
                        ? userProfile.Name
                        : userProfile.DisplayName,
                    IsDefault = userProfile.IsDefault,
                    WillPromoteReplacement = userProfile.IsDefault && replacement is not null,
                    ReplacementUserProfileId = replacement?.Id,
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
                var userProfile = await dbContext.UserProfiles
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (userProfile is null)
                {
                    return Results.NotFound();
                }

                var wasDefault = userProfile.IsDefault;
                UserProfile? replacement = null;

                dbContext.UserProfiles.Remove(userProfile);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (wasDefault)
                {
                    replacement = await dbContext.UserProfiles
                        .OrderBy(x => x.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    await SyncDefaultUserProfileAsync(
                        dbContext,
                        replacement?.Id,
                        cancellationToken);
                }

                return Results.Ok(new
                {
                    deletedUserProfileId = id,
                    promotedReplacementUserProfileId = replacement?.Id,
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

    private static async Task SyncDefaultUserProfileAsync(
        ApplicationDbContext dbContext,
        Guid? defaultUserProfileId,
        CancellationToken cancellationToken)
    {
        var userProfiles = await dbContext.UserProfiles.ToListAsync(cancellationToken);

        foreach (var userProfile in userProfiles)
        {
            userProfile.IsDefault = defaultUserProfileId.HasValue && userProfile.Id == defaultUserProfileId.Value;
            userProfile.UpdatedAt = DateTime.UtcNow;
        }

        var defaults = await GetOrCreateAppDefaultsAsync(dbContext, cancellationToken);
        defaults.DefaultUserProfileId = defaultUserProfileId;
        defaults.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserProfileResponse ToResponse(UserProfile x) =>
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
