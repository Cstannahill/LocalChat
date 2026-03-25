using LocalChat.Application.Inspection;
using LocalChat.Contracts.Inspection;

namespace LocalChat.Api.Endpoints;

public static class MemoryExtractionInspectionEndpoints
{
    public static IEndpointRouteBuilder MapMemoryExtractionInspectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inspection/memory-extraction")
            .WithTags("Inspection");

        group.MapGet("/conversations/{conversationId:guid}", async (
            Guid conversationId,
            IMemoryExtractionInspectionService inspectionService,
            CancellationToken cancellationToken) =>
        {
            var result = await inspectionService.InspectConversationAsync(conversationId, cancellationToken);

            return Results.Ok(new MemoryExtractionAuditResponse
            {
                ConversationId = result.ConversationId,
                TotalEventCount = result.TotalEventCount,
                DurableEventCount = result.DurableEventCount,
                SceneStateEventCount = result.SceneStateEventCount,
                Events = result.Events.Select(x => new MemoryExtractionAuditItemResponse
                {
                    EventId = x.EventId,
                    Category = x.Category,
                    Kind = x.Kind,
                    SlotFamily = x.SlotFamily,
                    SlotKey = x.SlotKey,
                    CandidateContent = x.CandidateContent,
                    Action = x.Action,
                    ConfidenceScore = x.ConfidenceScore,
                    ExistingMemoryItemId = x.ExistingMemoryItemId,
                    ExistingMemoryContent = x.ExistingMemoryContent,
                    Notes = x.Notes,
                    CreatedAt = x.CreatedAt
                }).ToList()
            });
        });

        return app;
    }
}
