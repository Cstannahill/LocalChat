using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Contracts.Settings;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class AppRuntimeDefaultsEndpoints
{
    public static IEndpointRouteBuilder MapAppRuntimeDefaultsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/app-defaults")
            .WithTags("Settings");

        group.MapGet("", async (
            IAppRuntimeDefaultsRepository repository,
            CancellationToken cancellationToken) =>
        {
            var defaults = await repository.GetOrCreateAsync(cancellationToken);

            return Results.Ok(new AppRuntimeDefaultsResponse
            {
                DefaultUserProfileId = defaults.DefaultUserProfileId,
                DefaultModelProfileId = defaults.DefaultModelProfileId,
                DefaultGenerationPresetId = defaults.DefaultGenerationPresetId
            });
        });

        group.MapPut("", async (
            UpdateAppRuntimeDefaultsRequest request,
            IAppRuntimeDefaultsRepository repository,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var defaults = await repository.GetOrCreateAsync(cancellationToken);

            defaults.DefaultUserProfileId = request.DefaultUserProfileId;
            defaults.DefaultModelProfileId = request.DefaultModelProfileId;
            defaults.DefaultGenerationPresetId = request.DefaultGenerationPresetId;
            defaults.UpdatedAt = DateTime.UtcNow;

            var userProfiles = await dbContext.UserProfiles.ToListAsync(cancellationToken);
            foreach (var userProfile in userProfiles)
            {
                userProfile.IsDefault = request.DefaultUserProfileId.HasValue &&
                                    userProfile.Id == request.DefaultUserProfileId.Value;
                userProfile.UpdatedAt = DateTime.UtcNow;
            }

            await repository.SaveChangesAsync(cancellationToken);

            return Results.Ok(new AppRuntimeDefaultsResponse
            {
                DefaultUserProfileId = defaults.DefaultUserProfileId,
                DefaultModelProfileId = defaults.DefaultModelProfileId,
                DefaultGenerationPresetId = defaults.DefaultGenerationPresetId
            });
        });

        return app;
    }
}
