using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Contracts.Inspection;
using LocalChat.Contracts.Lorebooks;
using LocalChat.Contracts.Memory;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;

namespace LocalChat.Api.Endpoints;

public static class InspectionEndpoints
{
    public static IEndpointRouteBuilder MapInspectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inspection").WithTags("Inspection");

        group.MapPost(
            "/retrieval",
            async (
                RetrievalInspectionRequest request,
                IRetrievalService retrievalService,
                CancellationToken cancellationToken
            ) =>
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return Results.BadRequest(new { error = "Query cannot be empty." });
                }

                var inspection = await retrievalService.InspectAsync(
                    request.CharacterId,
                    request.ConversationId,
                    request.Query,
                    cancellationToken
                );

                var response = new RetrievalInspectionResponse
                {
                    Query = inspection.Query,
                    SelectedMemories = inspection
                        .SelectedMemories.Select(ToMemoryResponse)
                        .ToList(),
                    SelectedLoreEntries = inspection
                        .SelectedLoreEntries.Select(ToLoreEntryResponse)
                        .ToList(),
                    SelectedMemoryExplanations = inspection.SelectedMemoryExplanations
                        .Select(x => new SelectedMemoryExplanationResponse
                        {
                            MemoryId = x.MemoryId,
                            Category = x.Category,
                            Kind = x.Kind,
                            Content = x.Content,
                            SlotKey = x.SlotKey,
                            SemanticScore = x.SemanticScore,
                            FinalScore = x.FinalScore,
                            WhySelected = x.WhySelected,
                            SuppressedMemories = x.SuppressedMemories
                                .Select(s => new SuppressedMemoryResponse
                                {
                                    MemoryId = s.MemoryId,
                                    Category = s.Category,
                                    Kind = s.Kind,
                                    Content = s.Content,
                                    SlotKey = s.SlotKey,
                                    FinalScore = s.FinalScore,
                                    Reason = s.Reason
                                })
                                .ToList()
                        })
                        .ToList(),
                    SelectedLoreExplanations = inspection.SelectedLoreExplanations
                        .Select(x => new SelectedLoreExplanationResponse
                        {
                            LoreEntryId = x.LoreEntryId,
                            Title = x.Title,
                            Content = x.Content,
                            SemanticScore = x.SemanticScore,
                            FinalScore = x.FinalScore,
                            WhySelected = x.WhySelected
                        })
                        .ToList()
                };

                return Results.Ok(response);
            }
        );

        group.MapPost(
            "/prompt",
            async (
                PromptInspectionRequest request,
                IPromptInspectionService promptInspectionService,
                CancellationToken cancellationToken
            ) =>
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return Results.BadRequest(new { error = "Query cannot be empty." });
                }

                var inspection = await promptInspectionService.InspectAsync(
                    request.CharacterId,
                    request.ConversationId,
                    request.UserPersonaId,
                    request.Query,
                    cancellationToken
                );

                var response = new PromptInspectionResponse
                {
                    Query = inspection.Query,
                    ModelName = inspection.ModelName,
                    ModelProfileName = inspection.ModelProfileName,
                    GenerationPresetName = inspection.GenerationPresetName,
                    EffectiveContextLength = inspection.EffectiveContextLength,
                    MaxPromptTokens = inspection.MaxPromptTokens,
                    EstimatedPromptTokens = inspection.EstimatedPromptTokens,
                    FitsWithinBudget = inspection.FitsWithinBudget,
                    FinalPrompt = inspection.FinalPrompt,
                    CharacterDefinitionSection = FindSection(
                        inspection.Sections,
                        "Character Definition"
                    ),
                    CharacterScenarioSection = FindSection(
                        inspection.Sections,
                        "Character Scenario"
                    ),
                    SampleDialogueSection = FindSection(
                        inspection.Sections,
                        "Sample Dialogue Examples"
                    ),
                    UserPersonaSection = FindSection(inspection.Sections, "User Persona"),
                    DirectorSection = FindSection(inspection.Sections, "Director Instructions"),
                    SceneContextSection = FindSection(inspection.Sections, "Scene Context"),
                    OocModeSection = FindSection(inspection.Sections, "OOC Mode"),
                    Sections = inspection
                        .Sections.Select(x => new PromptSectionResponse
                        {
                            Name = x.Name,
                            Content = x.Content,
                            EstimatedTokens = x.EstimatedTokens,
                        })
                        .ToList(),
                    SelectedSceneState = inspection.SelectedSceneState
                        .Select(x => new PromptSceneStateSelectedDebugResponse
                        {
                            MemoryId = x.MemoryId,
                            SlotFamily = x.SlotFamily,
                            SlotKey = x.SlotKey,
                            Content = x.Content,
                            PromptContent = x.PromptContent
                        })
                        .ToList(),
                    SuppressedSceneState = inspection.SuppressedSceneState
                        .Select(x => new PromptSceneStateSuppressedDebugResponse
                        {
                            MemoryId = x.MemoryId,
                            SlotFamily = x.SlotFamily,
                            Content = x.Content,
                            Reason = x.Reason
                        })
                        .ToList(),
                    SelectedDurableMemory = inspection.SelectedDurableMemory
                        .Select(x => new PromptDurableMemorySelectedDebugResponse
                        {
                            MemoryId = x.MemoryId,
                            Category = x.Category,
                            Content = x.Content,
                            PromptContent = x.PromptContent
                        })
                        .ToList(),
                    SuppressedDurableMemory = inspection.SuppressedDurableMemory
                        .Select(x => new PromptDurableMemorySuppressedDebugResponse
                        {
                            MemoryId = x.MemoryId,
                            Category = x.Category,
                            Content = x.Content,
                            Reason = x.Reason
                        })
                        .ToList()
                };

                return Results.Ok(response);
            }
        );

        group.MapPost(
            "/summary",
            async (
                SummaryInspectionRequest request,
                IPromptInspectionService promptInspectionService,
                CancellationToken cancellationToken
            ) =>
            {
                var query = string.IsNullOrWhiteSpace(request.Query)
                    ? "Continue the conversation."
                    : request.Query;

                var inspection = await promptInspectionService.InspectAsync(
                    request.CharacterId,
                    request.ConversationId,
                    null,
                    query,
                    cancellationToken
                );

                var response = new SummaryInspectionResponse
                {
                    ConversationId = request.ConversationId,
                    HasSummaryCheckpoint = inspection.LatestSummaryCheckpointId.HasValue,
                    SummaryUsedInPrompt = inspection.SummaryUsedInPrompt,
                    SummaryCheckpointId = inspection.LatestSummaryCheckpointId,
                    SummaryCreatedAt = inspection.LatestSummaryCreatedAt,
                    StartSequenceNumber = inspection.SummaryStartSequenceNumber,
                    EndSequenceNumber = inspection.SummaryEndSequenceNumber,
                    SummaryCoveredMessageCount = inspection.SummaryCoveredMessageCount,
                    TotalPriorMessageCount = inspection.TotalPriorMessageCount,
                    IncludedRawMessageCount = inspection.IncludedRawMessageCount,
                    ExcludedRawMessageCount = inspection.ExcludedRawMessageCount,
                    SummaryText = inspection.RollingSummary,
                };

                return Results.Ok(response);
            }
        );

        return app;
    }

    private static string? FindSection(
        IReadOnlyList<LocalChat.Application.Prompting.Composition.PromptSection> sections,
        string name
    )
    {
        return sections.FirstOrDefault(x => x.Name == name)?.Content;
    }

    private static MemoryItemResponse ToMemoryResponse(MemoryItem memory)
    {
        return new MemoryItemResponse
        {
            Id = memory.Id,
            Category = memory.Category.ToString(),
            Kind = memory.Kind.ToString(),
            ScopeType = memory.ScopeType.ToString(),
            Content = memory.Content,
            ReviewStatus = memory.ReviewStatus.ToString(),
            IsPinned = memory.IsPinned,
            ConfidenceScore = memory.ConfidenceScore,
            ProposalReason = memory.ProposalReason,
            SourceExcerpt = memory.SourceExcerpt,
            NormalizedKey = memory.NormalizedKey,
            SlotKey = memory.SlotKey,
            SlotFamily = memory.SlotFamily.ToString(),
            ConflictsWithMemoryItemId = memory.ConflictsWithMemoryItemId,
            SourceMessageSequenceNumber = memory.SourceMessageSequenceNumber,
            LastObservedSequenceNumber = memory.LastObservedSequenceNumber,
            SupersededAtSequenceNumber = memory.SupersededAtSequenceNumber,
            ExpiresAt = memory.ExpiresAt,
            CreatedAt = memory.CreatedAt,
            UpdatedAt = memory.UpdatedAt
        };
    }

    private static LoreEntryResponse ToLoreEntryResponse(LoreEntry loreEntry)
    {
        return new LoreEntryResponse
        {
            Id = loreEntry.Id,
            LorebookId = loreEntry.LorebookId,
            Title = loreEntry.Title,
            Content = loreEntry.Content,
            IsEnabled = loreEntry.IsEnabled,
            CreatedAt = loreEntry.CreatedAt,
            UpdatedAt = loreEntry.UpdatedAt
        };
    }
}
