using LocalChat.Application.Authoring;
using LocalChat.Contracts.Authoring;

namespace LocalChat.Api.Endpoints;

public static class AuthoringAssistantEndpoints
{
    public static IEndpointRouteBuilder MapAuthoringAssistantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authoring")
            .WithTags("Authoring");

        group.MapGet("/templates", async (
            string entityType,
            string? fieldName,
            IAuthoringAssistantService authoringAssistantService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return Results.BadRequest("entityType is required.");
            }

            var templates = await authoringAssistantService.GetTemplatesAsync(
                entityType.Trim(),
                fieldName?.Trim(),
                cancellationToken);

            var response = templates
                .Select(x => new AuthoringFieldTemplateResponse
                {
                    EntityType = x.EntityType,
                    FieldName = x.FieldName,
                    Title = x.Title,
                    Summary = x.Summary,
                    Content = x.Content
                })
                .ToList();

            return Results.Ok(response);
        });

        group.MapGet("/starter-packs", async (
            IAuthoringAssistantService authoringAssistantService,
            CancellationToken cancellationToken) =>
        {
            var packs = await authoringAssistantService.GetStarterPacksAsync(cancellationToken);

            return Results.Ok(packs.Select(x => new AuthoringStarterPackResponse
            {
                Id = x.Id,
                Title = x.Title,
                Summary = x.Summary,
                Concept = x.Concept,
                Vibe = x.Vibe,
                Relationship = x.Relationship,
                Setting = x.Setting
            }).ToList());
        });

        group.MapPost("/enhance", async (
            AuthoringEnhancementRequest request,
            IAuthoringAssistantService authoringAssistantService,
            CancellationToken cancellationToken) =>
        {
            var result = await authoringAssistantService.EnhanceAsync(
                new AuthoringEnhancementInput
                {
                    EntityType = request.EntityType.Trim(),
                    FieldName = request.FieldName.Trim(),
                    CurrentText = request.CurrentText ?? string.Empty,
                    Mode = string.IsNullOrWhiteSpace(request.Mode) ? "clarify" : request.Mode.Trim(),
                    ModelOverride = request.ModelOverride,
                    Context = request.Context ?? new Dictionary<string, string>()
                },
                cancellationToken);

            return Results.Ok(new AuthoringEnhancementResponse
            {
                EntityType = result.EntityType,
                FieldName = result.FieldName,
                Mode = result.Mode,
                OriginalText = result.OriginalText,
                SuggestedText = result.SuggestedText,
                Rationale = result.Rationale
            });
        });

        group.MapPost("/generate-full-bundle", async (
            FullAuthoringBundleGenerationRequest request,
            IAuthoringAssistantService authoringAssistantService,
            CancellationToken cancellationToken) =>
        {
            var result = await authoringAssistantService.GenerateFullBundleFromBriefAsync(
                new FullAuthoringBundleGenerationInput
                {
                    Concept = request.Concept.Trim(),
                    Vibe = request.Vibe,
                    Relationship = request.Relationship,
                    Setting = request.Setting,
                    ModelOverride = request.ModelOverride,
                    ExistingContext = request.ExistingContext ?? new Dictionary<string, string>()
                },
                cancellationToken);

            return Results.Ok(new FullAuthoringBundleGenerationResponse
            {
                CharacterName = result.CharacterName,
                CharacterDescription = result.CharacterDescription,
                CharacterPersonalityDefinition = result.CharacterPersonalityDefinition,
                CharacterScenario = result.CharacterScenario,
                CharacterGreeting = result.CharacterGreeting,
                PersonaDisplayName = result.PersonaDisplayName,
                PersonaDescription = result.PersonaDescription,
                PersonaTraits = result.PersonaTraits,
                PersonaPreferences = result.PersonaPreferences,
                PersonaAdditionalInstructions = result.PersonaAdditionalInstructions,
                Rationale = result.Rationale
            });
        });

        group.MapPost("/consistency-check", async (
            AuthoringConsistencyCheckRequest request,
            IAuthoringAssistantService authoringAssistantService,
            CancellationToken cancellationToken) =>
        {
            var result = await authoringAssistantService.CheckConsistencyAsync(
                new AuthoringConsistencyCheckInput
                {
                    EntityType = request.EntityType.Trim(),
                    Fields = request.Fields ?? new Dictionary<string, string>(),
                    ModelOverride = request.ModelOverride
                },
                cancellationToken);

            return Results.Ok(new AuthoringConsistencyCheckResponse
            {
                EntityType = result.EntityType,
                Summary = result.Summary,
                Issues = result.Issues
                    .Select(x => new AuthoringConsistencyIssueResponse
                    {
                        Severity = x.Severity,
                        FieldName = x.FieldName,
                        IssueType = x.IssueType,
                        Description = x.Description,
                        Suggestion = x.Suggestion
                    })
                    .ToList()
            });
        });

        group.MapPost("/repair-issue", async (
            ConsistencyIssueRepairRequest request,
            IAuthoringAssistantService authoringAssistantService,
            CancellationToken cancellationToken) =>
        {
            var result = await authoringAssistantService.RepairConsistencyIssueAsync(
                new ConsistencyIssueRepairInput
                {
                    EntityType = request.EntityType.Trim(),
                    FieldName = request.FieldName.Trim(),
                    IssueType = request.IssueType.Trim(),
                    IssueDescription = request.IssueDescription.Trim(),
                    SuggestedFixHint = request.SuggestedFixHint,
                    CurrentText = request.CurrentText ?? string.Empty,
                    ModelOverride = request.ModelOverride,
                    Context = request.Context ?? new Dictionary<string, string>()
                },
                cancellationToken);

            return Results.Ok(new AuthoringEnhancementResponse
            {
                EntityType = result.EntityType,
                FieldName = result.FieldName,
                Mode = result.Mode,
                OriginalText = result.OriginalText,
                SuggestedText = result.SuggestedText,
                Rationale = result.Rationale
            });
        });

        return app;
    }
}
