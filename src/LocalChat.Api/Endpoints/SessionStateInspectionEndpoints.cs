using LocalChat.Application.Inspection;
using LocalChat.Contracts.Inspection;

namespace LocalChat.Api.Endpoints;

public static class SessionStateInspectionEndpoints
{
    public static IEndpointRouteBuilder MapSessionStateInspectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inspection/session-state")
            .WithTags("Inspection");

        group.MapGet("/conversations/{conversationId:guid}", async (
            Guid conversationId,
            ISessionStateInspectionService inspectionService,
            CancellationToken cancellationToken) =>
        {
            var result = await inspectionService.InspectConversationAsync(conversationId, cancellationToken);

            return Results.Ok(new SessionStateInspectionResponse
            {
                ConversationId = result.ConversationId,
                ActiveSessionState = result.ActiveSessionState
                    .Select(x => new SessionStateDebugItemResponse
                    {
                        MemoryId = x.MemoryId,
                        Content = x.Content,
                        SlotFamily = x.SlotFamily,
                        SlotKey = x.SlotKey,
                        ReviewStatus = x.ReviewStatus,
                        ExpiresAt = x.ExpiresAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToList(),
                ReplacementHistory = result.ReplacementHistory
                    .Select(x => new SessionStateReplacementHistoryItemResponse
                    {
                        EventId = x.EventId,
                        SlotFamily = x.SlotFamily,
                        SlotKey = x.SlotKey,
                        CandidateContent = x.CandidateContent,
                        Action = x.Action,
                        ReplacedMemoryItemId = x.ReplacedMemoryItemId,
                        ReplacedMemoryContent = x.ReplacedMemoryContent,
                        Notes = x.Notes,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList(),
                FamilyCollisions = result.FamilyCollisions
                    .Select(x => new SessionStateReplacementHistoryItemResponse
                    {
                        EventId = x.EventId,
                        SlotFamily = x.SlotFamily,
                        SlotKey = x.SlotKey,
                        CandidateContent = x.CandidateContent,
                        Action = x.Action,
                        ReplacedMemoryItemId = x.ReplacedMemoryItemId,
                        ReplacedMemoryContent = x.ReplacedMemoryContent,
                        Notes = x.Notes,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList()
            });
        });

        return app;
    }
}
