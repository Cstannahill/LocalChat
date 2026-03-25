using LocalChat.Application.Abstractions.ImageGeneration;
using LocalChat.Application.ImageGeneration;
using LocalChat.Contracts.Images;
using LocalChat.Domain.Entities.Images;

namespace LocalChat.Api.Endpoints;

public static class ImageGenerationEndpoints
{
    public static IEndpointRouteBuilder MapImageGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/images")
            .WithTags("Images");

        group.MapPost("/contextual-prompt", async (
            GenerateContextualImagePromptRequest request,
            ConversationVisualPromptService visualPromptService,
            CancellationToken cancellationToken) =>
        {
            ContextualImagePromptResult result;
            try
            {
                result = await visualPromptService.GenerateAsync(
                    request.ConversationId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }

            return Results.Ok(new ContextualImagePromptResponse
            {
                ConversationId = result.ConversationId,
                PositivePrompt = result.PositivePrompt,
                NegativePrompt = result.NegativePrompt,
                SceneSummary = result.SceneSummary,
                AssumptionsOrUnknowns = result.AssumptionsOrUnknowns
            });
        });

        group.MapPost("/generate", async (
            GenerateImageRequest request,
            ImageGenerationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Results.BadRequest(new { error = "Prompt is required." });
            }

            var job = await orchestrator.GenerateAsync(
                request.ConversationId,
                new ImageGenerationRequest
                {
                    Prompt = request.Prompt.Trim(),
                    NegativePrompt = request.NegativePrompt?.Trim(),
                    Width = request.Width,
                    Height = request.Height,
                    Steps = request.Steps,
                    Cfg = request.Cfg,
                    Seed = request.Seed
                },
                cancellationToken);

            return Results.Ok(ToResponse(job));
        });

        group.MapGet("/conversations/{conversationId:guid}", async (
            Guid conversationId,
            ImageGenerationOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            var jobs = await orchestrator.ListByConversationAsync(conversationId, cancellationToken);
            return Results.Ok(jobs.Select(ToResponse).ToList());
        });

        return app;
    }

    private static ImageGenerationJobResponse ToResponse(ImageGenerationJob x) =>
        new()
        {
            Id = x.Id,
            CharacterId = x.CharacterId,
            ConversationId = x.ConversationId,
            Provider = x.Provider,
            PromptText = x.PromptText,
            NegativePromptText = x.NegativePromptText,
            Width = x.Width,
            Height = x.Height,
            Steps = x.Steps,
            Cfg = x.Cfg,
            Seed = x.Seed,
            Status = x.Status.ToString(),
            ErrorMessage = x.ErrorMessage,
            ProviderJobId = x.ProviderJobId,
            CreatedAt = x.CreatedAt,
            CompletedAt = x.CompletedAt,
            Assets = x.Assets
                .OrderBy(a => a.SortOrder)
                .Select(a => new GeneratedImageAssetResponse
                {
                    Id = a.Id,
                    Url = a.RelativeUrl,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    SortOrder = a.SortOrder,
                    CreatedAt = a.CreatedAt
                })
                .ToList()
        };
}
