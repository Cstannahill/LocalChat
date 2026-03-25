using LocalChat.Application.Chat;
using LocalChat.Contracts.Chat;

namespace LocalChat.Api.Endpoints;

public static class SuggestedUserMessageEndpoints
{
    public static IEndpointRouteBuilder MapSuggestedUserMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
            .WithTags("Chat");

        group.MapPost("/suggest-user-message", async (
            GenerateSuggestedUserMessageRequest request,
            UserMessageSuggestionService suggestionService,
            CancellationToken cancellationToken) =>
        {
            var result = await suggestionService.GenerateAsync(
                request.ConversationId,
                cancellationToken);

            return Results.Ok(new GenerateSuggestedUserMessageResponse
            {
                ConversationId = result.ConversationId,
                SuggestedMessage = result.SuggestedMessage,
                Tone = result.Tone,
                ReasoningSummary = result.ReasoningSummary
            });
        });

        return app;
    }
}
