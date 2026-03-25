using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Contracts.KnowledgeBases;
using LocalChat.Domain.Entities.KnowledgeBases;

namespace LocalChat.Api.Endpoints;

public static class KnowledgeBaseEndpoints
{
    public static IEndpointRouteBuilder MapKnowledgeBaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/knowledge-bases").WithTags("KnowledgeBases");

        group.MapGet(
            "/",
            async (
                Guid? agentId,
                IKnowledgeBaseRepository repository,
                CancellationToken cancellationToken
            ) =>
            {
                var knowledgeBases = await repository.ListKnowledgeBasesAsync(agentId, cancellationToken);

                var response = knowledgeBases.Select(x => new KnowledgeBaseResponse
                {
                    Id = x.Id,
                    AgentId = x.AgentId,
                    Name = x.Name,
                    Description = x.Description,
                    Entries = x
                        .Entries.OrderBy(e => e.Title)
                        .Select(e => new LoreEntryResponse
                        {
                            Id = e.Id,
                            KnowledgeBaseId = e.KnowledgeBaseId,
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
            async (Guid id, IKnowledgeBaseRepository repository, CancellationToken cancellationToken) =>
            {
                var knowledgeBase = await repository.GetKnowledgeBaseByIdAsync(id, cancellationToken);
                if (knowledgeBase is null)
                {
                    return Results.NotFound();
                }

                var response = new KnowledgeBaseResponse
                {
                    Id = knowledgeBase.Id,
                    AgentId = knowledgeBase.AgentId,
                    Name = knowledgeBase.Name,
                    Description = knowledgeBase.Description,
                    Entries = knowledgeBase
                        .Entries.OrderBy(e => e.Title)
                        .Select(e => new LoreEntryResponse
                        {
                            Id = e.Id,
                            KnowledgeBaseId = e.KnowledgeBaseId,
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
                CreateKnowledgeBaseRequest request,
                IKnowledgeBaseRepository repository,
                CancellationToken cancellationToken
            ) =>
            {
                var knowledgeBase = new KnowledgeBase
                {
                    Id = Guid.NewGuid(),
                    AgentId = request.AgentId,
                    Name = request.Name.Trim(),
                    Description = request.Description.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await repository.AddKnowledgeBaseAsync(knowledgeBase, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                return Results.Ok(
                    new KnowledgeBaseResponse
                    {
                        Id = knowledgeBase.Id,
                        AgentId = knowledgeBase.AgentId,
                        Name = knowledgeBase.Name,
                        Description = knowledgeBase.Description,
                        Entries = Array.Empty<LoreEntryResponse>(),
                    }
                );
            }
        );

        group.MapPost(
            "/entries",
            async (
                CreateLoreEntryRequest request,
                IKnowledgeBaseRepository repository,
                IRetrievalService retrievalService,
                CancellationToken cancellationToken
            ) =>
            {
                var knowledgeBase = await repository.GetKnowledgeBaseByIdAsync(
                    request.KnowledgeBaseId,
                    cancellationToken
                );
                if (knowledgeBase is null)
                {
                    return Results.BadRequest(
                        new { error = $"KnowledgeBase '{request.KnowledgeBaseId}' was not found." }
                    );
                }

                var entry = new LoreEntry
                {
                    Id = Guid.NewGuid(),
                    KnowledgeBaseId = knowledgeBase.Id,
                    Title = request.Title.Trim(),
                    Content = request.Content.Trim(),
                    IsEnabled = request.IsEnabled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await repository.AddLoreEntryAsync(entry, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                if (knowledgeBase.AgentId.HasValue)
                {
                    await retrievalService.IndexLoreEntryAsync(
                        knowledgeBase.AgentId.Value,
                        entry,
                        cancellationToken
                    );
                }

                return Results.Ok(
                    new LoreEntryResponse
                    {
                        Id = entry.Id,
                        KnowledgeBaseId = entry.KnowledgeBaseId,
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
                IKnowledgeBaseRepository repository,
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

                if (entry.KnowledgeBase?.AgentId is Guid agentId)
                {
                    if (entry.IsEnabled)
                    {
                        await retrievalService.IndexLoreEntryAsync(
                            agentId,
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
                        KnowledgeBaseId = entry.KnowledgeBaseId,
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
