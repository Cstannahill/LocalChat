using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Contracts.Lorebooks;
using LocalChat.Domain.Entities.Lorebooks;

namespace LocalChat.Api.Endpoints;

public static class LorebookEndpoints
{
    public static IEndpointRouteBuilder MapLorebookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lorebooks").WithTags("Lorebooks");

        group.MapGet(
            "/",
            async (
                Guid? characterId,
                ILorebookRepository repository,
                CancellationToken cancellationToken
            ) =>
            {
                var lorebooks = await repository.ListLorebooksAsync(characterId, cancellationToken);

                var response = lorebooks.Select(x => new LorebookResponse
                {
                    Id = x.Id,
                    CharacterId = x.CharacterId,
                    Name = x.Name,
                    Description = x.Description,
                    Entries = x
                        .Entries.OrderBy(e => e.Title)
                        .Select(e => new LoreEntryResponse
                        {
                            Id = e.Id,
                            LorebookId = e.LorebookId,
                            Title = e.Title,
                            Content = e.Content,
                            IsEnabled = e.IsEnabled,
                            CreatedAt = e.CreatedAt,
                            UpdatedAt = e.UpdatedAt,
                        })
                        .ToList(),
                });

                return Results.Ok(response);
            }
        );

        group.MapGet(
            "/{id:guid}",
            async (Guid id, ILorebookRepository repository, CancellationToken cancellationToken) =>
            {
                var lorebook = await repository.GetLorebookByIdAsync(id, cancellationToken);
                if (lorebook is null)
                {
                    return Results.NotFound();
                }

                var response = new LorebookResponse
                {
                    Id = lorebook.Id,
                    CharacterId = lorebook.CharacterId,
                    Name = lorebook.Name,
                    Description = lorebook.Description,
                    Entries = lorebook
                        .Entries.OrderBy(e => e.Title)
                        .Select(e => new LoreEntryResponse
                        {
                            Id = e.Id,
                            LorebookId = e.LorebookId,
                            Title = e.Title,
                            Content = e.Content,
                            IsEnabled = e.IsEnabled,
                            CreatedAt = e.CreatedAt,
                            UpdatedAt = e.UpdatedAt,
                        })
                        .ToList(),
                };

                return Results.Ok(response);
            }
        );

        group.MapPost(
            "/",
            async (
                CreateLorebookRequest request,
                ILorebookRepository repository,
                CancellationToken cancellationToken
            ) =>
            {
                var lorebook = new Lorebook
                {
                    Id = Guid.NewGuid(),
                    CharacterId = request.CharacterId,
                    Name = request.Name.Trim(),
                    Description = request.Description.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await repository.AddLorebookAsync(lorebook, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                return Results.Ok(
                    new LorebookResponse
                    {
                        Id = lorebook.Id,
                        CharacterId = lorebook.CharacterId,
                        Name = lorebook.Name,
                        Description = lorebook.Description,
                        Entries = Array.Empty<LoreEntryResponse>(),
                    }
                );
            }
        );

        group.MapPost(
            "/entries",
            async (
                CreateLoreEntryRequest request,
                ILorebookRepository repository,
                IRetrievalService retrievalService,
                CancellationToken cancellationToken
            ) =>
            {
                var lorebook = await repository.GetLorebookByIdAsync(
                    request.LorebookId,
                    cancellationToken
                );
                if (lorebook is null)
                {
                    return Results.BadRequest(
                        new { error = $"Lorebook '{request.LorebookId}' was not found." }
                    );
                }

                var entry = new LoreEntry
                {
                    Id = Guid.NewGuid(),
                    LorebookId = lorebook.Id,
                    Title = request.Title.Trim(),
                    Content = request.Content.Trim(),
                    IsEnabled = request.IsEnabled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await repository.AddLoreEntryAsync(entry, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                if (lorebook.CharacterId.HasValue)
                {
                    await retrievalService.IndexLoreEntryAsync(
                        lorebook.CharacterId.Value,
                        entry,
                        cancellationToken
                    );
                }

                return Results.Ok(
                    new LoreEntryResponse
                    {
                        Id = entry.Id,
                        LorebookId = entry.LorebookId,
                        Title = entry.Title,
                        Content = entry.Content,
                        IsEnabled = entry.IsEnabled,
                        CreatedAt = entry.CreatedAt,
                        UpdatedAt = entry.UpdatedAt,
                    }
                );
            }
        );

        group.MapPut(
            "/entries/{id:guid}",
            async (
                Guid id,
                UpdateLoreEntryRequest request,
                ILorebookRepository repository,
                IRetrievalService retrievalService,
                CancellationToken cancellationToken
            ) =>
            {
                var entry = await repository.GetLoreEntryByIdAsync(id, cancellationToken);
                if (entry is null)
                {
                    return Results.NotFound();
                }

                entry.Title = request.Title.Trim();
                entry.Content = request.Content.Trim();
                entry.IsEnabled = request.IsEnabled;
                entry.UpdatedAt = DateTime.UtcNow;

                await repository.SaveChangesAsync(cancellationToken);

                if (entry.Lorebook?.CharacterId is Guid characterId)
                {
                    if (entry.IsEnabled)
                    {
                        await retrievalService.IndexLoreEntryAsync(
                            characterId,
                            entry,
                            cancellationToken
                        );
                    }
                    else
                    {
                        await retrievalService.RemoveSourceAsync(
                            "Lore",
                            entry.Id,
                            cancellationToken
                        );
                    }
                }

                return Results.Ok(
                    new LoreEntryResponse
                    {
                        Id = entry.Id,
                        LorebookId = entry.LorebookId,
                        Title = entry.Title,
                        Content = entry.Content,
                        IsEnabled = entry.IsEnabled,
                        CreatedAt = entry.CreatedAt,
                        UpdatedAt = entry.UpdatedAt,
                    }
                );
            }
        );

        return app;
    }
}
