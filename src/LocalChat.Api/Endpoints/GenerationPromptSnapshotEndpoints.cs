using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Contracts.PromptSnapshots;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Api.Endpoints;

public static class GenerationPromptSnapshotEndpoints
{
    public static IEndpointRouteBuilder MapGenerationPromptSnapshotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prompt-snapshots")
            .WithTags("Prompt Snapshots");

        group.MapGet("/variants/{messageVariantId:guid}", async (
            Guid messageVariantId,
            IGenerationPromptSnapshotRepository snapshotRepository,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await snapshotRepository.GetByMessageVariantIdAsync(messageVariantId, cancellationToken);
            if (snapshot is null)
            {
                return Results.NotFound();
            }

            var assistantCompletion = await dbContext.MessageVariants
                .AsNoTracking()
                .Where(x => x.Id == messageVariantId)
                .Select(x => x.Content)
                .FirstOrDefaultAsync(cancellationToken);

            return Results.Ok(ToResponse(snapshot, assistantCompletion));
        });

        group.MapGet("/conversations/{conversationId:guid}", async (
            Guid conversationId,
            int? maxCount,
            IGenerationPromptSnapshotRepository snapshotRepository,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var take = maxCount.GetValueOrDefault(50);
            if (take <= 0)
            {
                take = 50;
            }

            var snapshots = await snapshotRepository.ListByConversationAsync(conversationId, take, cancellationToken);

            var variantIds = snapshots.Select(x => x.MessageVariantId).ToList();

            var completions = await dbContext.MessageVariants
                .AsNoTracking()
                .Where(x => variantIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Content, cancellationToken);

            var response = snapshots
                .Select(x => ToResponse(
                    x,
                    completions.TryGetValue(x.MessageVariantId, out var completion) ? completion : null))
                .ToList();

            return Results.Ok(response);
        });

        group.MapGet("/export", async (
            Guid? conversationId,
            string? conversationIdsCsv,
            bool? selectedOnly,
            string? provider,
            string? modelContains,
            DateTime? createdFromUtc,
            DateTime? createdToUtc,
            DateTime? conversationCreatedFromUtc,
            DateTime? conversationCreatedToUtc,
            int? maxCount,
            string? format,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            return await ExportSnapshotsAsync(
                dbContext,
                conversationId,
                conversationIdsCsv,
                selectedOnly,
                provider,
                modelContains,
                createdFromUtc,
                createdToUtc,
                conversationCreatedFromUtc,
                conversationCreatedToUtc,
                maxCount,
                format,
                cancellationToken);
        });

        group.MapGet("/export/conversations/{conversationId:guid}", async (
            Guid conversationId,
            string? conversationIdsCsv,
            bool? selectedOnly,
            string? provider,
            string? modelContains,
            DateTime? createdFromUtc,
            DateTime? createdToUtc,
            DateTime? conversationCreatedFromUtc,
            DateTime? conversationCreatedToUtc,
            int? maxCount,
            string? format,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            return await ExportSnapshotsAsync(
                dbContext,
                conversationId,
                conversationIdsCsv,
                selectedOnly,
                provider,
                modelContains,
                createdFromUtc,
                createdToUtc,
                conversationCreatedFromUtc,
                conversationCreatedToUtc,
                maxCount,
                format,
                cancellationToken);
        });

        return app;
    }

    private static async Task<IResult> ExportSnapshotsAsync(
        ApplicationDbContext dbContext,
        Guid? conversationId,
        string? conversationIdsCsv,
        bool? selectedOnly,
        string? provider,
        string? modelContains,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        DateTime? conversationCreatedFromUtc,
        DateTime? conversationCreatedToUtc,
        int? maxCount,
        string? format,
        CancellationToken cancellationToken)
    {
        var take = maxCount.GetValueOrDefault(500);
        if (take <= 0)
        {
            take = 500;
        }

        if (take > 5000)
        {
            take = 5000;
        }

        var normalizedFormat = NormalizeExportFormat(format);
        if (normalizedFormat is null)
        {
            return Results.BadRequest("format must be one of: json, jsonl, sharegpt, alpaca.");
        }

        var explicitConversationIds = ParseConversationIds(conversationIdsCsv);

        ProviderType? providerFilter = null;
        if (!string.IsNullOrWhiteSpace(provider))
        {
            if (!ModelRoute.TryParseProvider(provider, out var parsedProvider))
            {
                return Results.BadRequest("provider must be one of: ollama, openrouter, huggingface/hf, llama.cpp/llamacpp.");
            }

            providerFilter = parsedProvider;
        }

        var selectedOnlyValue = selectedOnly.GetValueOrDefault(false);
        var normalizedModelContains = string.IsNullOrWhiteSpace(modelContains)
            ? null
            : modelContains.Trim().ToLowerInvariant();

        var query =
            from snapshot in dbContext.GenerationPromptSnapshots.AsNoTracking()
            join variant in dbContext.MessageVariants.AsNoTracking()
                on snapshot.MessageVariantId equals variant.Id
            join message in dbContext.Messages.AsNoTracking()
                on snapshot.MessageId equals message.Id
            join conversation in dbContext.Conversations.AsNoTracking()
                on snapshot.ConversationId equals conversation.Id
            select new
            {
                Snapshot = snapshot,
                Variant = variant,
                Message = message,
                Conversation = conversation
            };

        if (conversationId.HasValue)
        {
            query = query.Where(x => x.Snapshot.ConversationId == conversationId.Value);
        }

        if (explicitConversationIds.Count > 0)
        {
            query = query.Where(x => explicitConversationIds.Contains(x.Snapshot.ConversationId));
        }

        if (providerFilter.HasValue)
        {
            query = query.Where(x => x.Snapshot.ProviderType == providerFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedModelContains))
        {
            query = query.Where(x =>
                x.Snapshot.ModelIdentifier != null &&
                x.Snapshot.ModelIdentifier.ToLower().Contains(normalizedModelContains));
        }

        if (createdFromUtc.HasValue)
        {
            query = query.Where(x => x.Snapshot.CreatedAt >= createdFromUtc.Value);
        }

        if (createdToUtc.HasValue)
        {
            query = query.Where(x => x.Snapshot.CreatedAt <= createdToUtc.Value);
        }

        if (conversationCreatedFromUtc.HasValue)
        {
            query = query.Where(x => x.Conversation.CreatedAt >= conversationCreatedFromUtc.Value);
        }

        if (conversationCreatedToUtc.HasValue)
        {
            query = query.Where(x => x.Conversation.CreatedAt <= conversationCreatedToUtc.Value);
        }

        if (selectedOnlyValue)
        {
            query = query.Where(x => x.Message.SelectedVariantIndex == x.Variant.VariantIndex);
        }

        var rows = await query
            .OrderByDescending(x => x.Snapshot.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var exportEntries = rows
            .Select(x => new ExportEntry(
                SnapshotId: x.Snapshot.Id,
                ConversationId: x.Snapshot.ConversationId,
                ConversationTitle: x.Conversation.Title,
                ConversationCreatedAt: x.Conversation.CreatedAt,
                MessageId: x.Snapshot.MessageId,
                SequenceNumber: x.Message.SequenceNumber,
                MessageVariantId: x.Snapshot.MessageVariantId,
                VariantIndex: x.Variant.VariantIndex,
                SelectedVariantIndex: x.Message.SelectedVariantIndex,
                IsSelectedVariant: x.Message.SelectedVariantIndex == x.Variant.VariantIndex,
                Provider: x.Snapshot.ProviderType.HasValue
                    ? ModelRoute.ProviderToWireValue(x.Snapshot.ProviderType.Value)
                    : null,
                ModelIdentifier: x.Snapshot.ModelIdentifier,
                ModelProfileId: x.Snapshot.ModelProfileId,
                GenerationPresetId: x.Snapshot.GenerationPresetId,
                RuntimeSource: x.Snapshot.RuntimeSourceType?.ToString(),
                EstimatedPromptTokens: x.Snapshot.EstimatedPromptTokens,
                ResolvedContextWindow: x.Snapshot.ResolvedContextWindow,
                SnapshotCreatedAt: x.Snapshot.CreatedAt,
                Prompt: x.Snapshot.FullPromptText,
                PromptSections: ParseSectionsForExport(x.Snapshot.PromptSectionsJson),
                Completion: x.Variant.Content))
            .ToList();

        var filtersPayload = new
        {
            conversationId,
            conversationIds = explicitConversationIds,
            selectedOnly = selectedOnlyValue,
            provider = providerFilter.HasValue
                ? ModelRoute.ProviderToWireValue(providerFilter.Value)
                : null,
            modelContains = modelContains,
            createdFromUtc,
            createdToUtc,
            conversationCreatedFromUtc,
            conversationCreatedToUtc,
            maxCount = take,
            format = normalizedFormat
        };

        return normalizedFormat switch
        {
            "json" => BuildJsonResult(exportEntries, filtersPayload),
            "jsonl" => BuildJsonlResult(exportEntries),
            "sharegpt" => BuildShareGptResult(exportEntries),
            "alpaca" => BuildAlpacaResult(exportEntries),
            _ => Results.BadRequest("Unsupported format.")
        };
    }

    private static IResult BuildJsonResult(
        IReadOnlyList<ExportEntry> exportEntries,
        object filtersPayload)
    {
        var payload = new
        {
            exportedAt = DateTime.UtcNow,
            filters = filtersPayload,
            count = exportEntries.Count,
            entries = exportEntries.Select(x => new
            {
                x.SnapshotId,
                x.ConversationId,
                x.ConversationTitle,
                x.ConversationCreatedAt,
                x.MessageId,
                x.SequenceNumber,
                x.MessageVariantId,
                x.VariantIndex,
                x.SelectedVariantIndex,
                x.IsSelectedVariant,
                x.Provider,
                x.ModelIdentifier,
                x.ModelProfileId,
                x.GenerationPresetId,
                x.RuntimeSource,
                x.EstimatedPromptTokens,
                x.ResolvedContextWindow,
                x.SnapshotCreatedAt,
                x.Prompt,
                promptSections = x.PromptSections,
                x.Completion
            }).ToList()
        };

        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var bytes = Encoding.UTF8.GetBytes(json);

        return Results.File(
            bytes,
            "application/json",
            $"prompt-snapshot-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    private static IResult BuildJsonlResult(IReadOnlyList<ExportEntry> exportEntries)
    {
        var lines = exportEntries.Select(x => JsonSerializer.Serialize(new
        {
            snapshotId = x.SnapshotId,
            conversationId = x.ConversationId,
            conversationTitle = x.ConversationTitle,
            conversationCreatedAt = x.ConversationCreatedAt,
            messageId = x.MessageId,
            sequenceNumber = x.SequenceNumber,
            messageVariantId = x.MessageVariantId,
            variantIndex = x.VariantIndex,
            selectedVariantIndex = x.SelectedVariantIndex,
            isSelectedVariant = x.IsSelectedVariant,
            provider = x.Provider,
            modelIdentifier = x.ModelIdentifier,
            modelProfileId = x.ModelProfileId,
            generationPresetId = x.GenerationPresetId,
            runtimeSource = x.RuntimeSource,
            estimatedPromptTokens = x.EstimatedPromptTokens,
            resolvedContextWindow = x.ResolvedContextWindow,
            snapshotCreatedAt = x.SnapshotCreatedAt,
            prompt = x.Prompt,
            promptSections = x.PromptSections,
            completion = x.Completion
        }));

        var content = string.Join('\n', lines);
        var bytes = Encoding.UTF8.GetBytes(content);

        return Results.File(
            bytes,
            "application/x-ndjson",
            $"prompt-snapshot-export-{DateTime.UtcNow:yyyyMMddHHmmss}.jsonl");
    }

    private static IResult BuildShareGptResult(IReadOnlyList<ExportEntry> exportEntries)
    {
        var payload = exportEntries.Select(x => new
        {
            id = x.MessageVariantId,
            conversations = new object[]
            {
                new
                {
                    from = "human",
                    value = x.Prompt
                },
                new
                {
                    from = "gpt",
                    value = x.Completion
                }
            },
            metadata = new
            {
                snapshotId = x.SnapshotId,
                x.ConversationId,
                x.ConversationTitle,
                x.ConversationCreatedAt,
                x.MessageId,
                x.SequenceNumber,
                x.VariantIndex,
                x.SelectedVariantIndex,
                x.IsSelectedVariant,
                x.Provider,
                x.ModelIdentifier,
                x.ModelProfileId,
                x.GenerationPresetId,
                x.RuntimeSource,
                x.EstimatedPromptTokens,
                x.ResolvedContextWindow,
                x.SnapshotCreatedAt,
                promptSections = x.PromptSections
            }
        }).ToList();

        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var bytes = Encoding.UTF8.GetBytes(json);

        return Results.File(
            bytes,
            "application/json",
            $"prompt-snapshot-sharegpt-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    private static IResult BuildAlpacaResult(IReadOnlyList<ExportEntry> exportEntries)
    {
        var payload = exportEntries.Select(x => new
        {
            instruction = string.Empty,
            input = x.Prompt,
            output = x.Completion,
            metadata = new
            {
                snapshotId = x.SnapshotId,
                x.ConversationId,
                x.ConversationTitle,
                x.ConversationCreatedAt,
                x.MessageId,
                x.SequenceNumber,
                x.VariantIndex,
                x.SelectedVariantIndex,
                x.IsSelectedVariant,
                x.Provider,
                x.ModelIdentifier,
                x.ModelProfileId,
                x.GenerationPresetId,
                x.RuntimeSource,
                x.EstimatedPromptTokens,
                x.ResolvedContextWindow,
                x.SnapshotCreatedAt,
                promptSections = x.PromptSections
            }
        }).ToList();

        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var bytes = Encoding.UTF8.GetBytes(json);

        return Results.File(
            bytes,
            "application/json",
            $"prompt-snapshot-alpaca-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
    }

    private static GenerationPromptSnapshotResponse ToResponse(
        LocalChat.Domain.Entities.Generation.GenerationPromptSnapshot snapshot,
        string? assistantCompletion)
    {
        var sections = ParseSections(snapshot.PromptSectionsJson);

        return new GenerationPromptSnapshotResponse
        {
            Id = snapshot.Id,
            MessageVariantId = snapshot.MessageVariantId,
            MessageId = snapshot.MessageId,
            ConversationId = snapshot.ConversationId,
            FullPromptText = snapshot.FullPromptText,
            Sections = sections,
            EstimatedPromptTokens = snapshot.EstimatedPromptTokens,
            ResolvedContextWindow = snapshot.ResolvedContextWindow,
            Provider = snapshot.ProviderType.HasValue
                ? ModelRoute.ProviderToWireValue(snapshot.ProviderType.Value)
                : null,
            ModelIdentifier = snapshot.ModelIdentifier,
            ModelProfileId = snapshot.ModelProfileId,
            GenerationPresetId = snapshot.GenerationPresetId,
            RuntimeSource = snapshot.RuntimeSourceType?.ToString(),
            CreatedAt = snapshot.CreatedAt,
            AssistantCompletion = assistantCompletion
        };
    }

    private static IReadOnlyList<PromptSnapshotSectionResponse> ParseSections(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<PromptSnapshotSectionResponse>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<PromptSnapshotSectionRecord>>(json)
                         ?? new List<PromptSnapshotSectionRecord>();

            return parsed
                .Select(x => new PromptSnapshotSectionResponse
                {
                    Name = x.Name,
                    Content = x.Content,
                    EstimatedTokens = x.EstimatedTokens
                })
                .ToList();
        }
        catch
        {
            return Array.Empty<PromptSnapshotSectionResponse>();
        }
    }

    private static IReadOnlyList<object> ParseSectionsForExport(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<object>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<PromptSnapshotSectionRecord>>(json)
                         ?? new List<PromptSnapshotSectionRecord>();

            return parsed
                .Select(x => (object)new
                {
                    x.Name,
                    x.Content,
                    x.EstimatedTokens
                })
                .ToList();
        }
        catch
        {
            return Array.Empty<object>();
        }
    }

    private static IReadOnlyList<Guid> ParseConversationIds(string? conversationIdsCsv)
    {
        if (string.IsNullOrWhiteSpace(conversationIdsCsv))
        {
            return Array.Empty<Guid>();
        }

        return conversationIdsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Guid.TryParse(x, out var parsed) ? parsed : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();
    }

    private static string? NormalizeExportFormat(string? format)
    {
        var value = string.IsNullOrWhiteSpace(format)
            ? "json"
            : format.Trim().ToLowerInvariant();

        return value switch
        {
            "json" => "json",
            "jsonl" => "jsonl",
            "sharegpt" => "sharegpt",
            "alpaca" => "alpaca",
            _ => null
        };
    }

    private sealed class PromptSnapshotSectionRecord
    {
        public string Name { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int EstimatedTokens { get; set; }
    }

    private sealed record ExportEntry(
        Guid SnapshotId,
        Guid ConversationId,
        string ConversationTitle,
        DateTime ConversationCreatedAt,
        Guid MessageId,
        int SequenceNumber,
        Guid MessageVariantId,
        int VariantIndex,
        int? SelectedVariantIndex,
        bool IsSelectedVariant,
        string? Provider,
        string? ModelIdentifier,
        Guid? ModelProfileId,
        Guid? GenerationPresetId,
        string? RuntimeSource,
        int EstimatedPromptTokens,
        int? ResolvedContextWindow,
        DateTime SnapshotCreatedAt,
        string Prompt,
        IReadOnlyList<object> PromptSections,
        string Completion);
}
