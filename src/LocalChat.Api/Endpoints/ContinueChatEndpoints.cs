using System.Text.Json;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Chat;
using LocalChat.Contracts.Chat;

namespace LocalChat.Api.Endpoints;

public static class ContinueChatEndpoints
{
    public static IEndpointRouteBuilder MapContinueChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
            .WithTags("Chat");

        group.MapPost("/continue/stream", async (
            ContinueConversationRequest request,
            string? overrideProvider,
            string? overrideModelIdentifier,
            IConversationRepository conversationRepository,
            ConversationContinuationService continuationService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var conversation = await conversationRepository.GetByIdWithMessagesAsync(
                request.ConversationId,
                cancellationToken);

            if (conversation is null)
            {
                return Results.NotFound(new { error = "Conversation not found." });
            }

            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.Headers.ContentType = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection = "keep-alive";
            httpContext.Response.Headers.Append("X-Accel-Buffering", "no");

            await WriteSseEventAsync(
                httpContext.Response,
                "started",
                new
                {
                    type = "started",
                    characterId = conversation.CharacterId,
                    conversationId = conversation.Id,
                    startedAt = DateTimeOffset.UtcNow
                },
                cancellationToken);

            try
            {
                var result = await continuationService.ContinueConversationAsync(
                    request.ConversationId,
                    async (delta, ct) =>
                    {
                        await WriteSseEventAsync(
                            httpContext.Response,
                            "token-delta",
                            new
                            {
                                type = "token-delta",
                                delta
                            },
                            ct);
                        },
                    overrideProvider,
                    overrideModelIdentifier,
                    cancellationToken);

                await WriteSseEventAsync(
                    httpContext.Response,
                    "completed",
                    new
                    {
                        type = "completed",
                        conversationId = result.ConversationId,
                        userMessageId = (Guid?)null,
                        assistantMessageId = result.AssistantMessageId,
                        assistantMessage = result.AssistantMessage,
                        conversationCreated = false
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await WriteSseEventAsync(
                    httpContext.Response,
                    "error",
                    new
                    {
                        type = "error",
                        message = ex.Message
                    },
                    cancellationToken);
            }

            return Results.Empty;
        });

        return app;
    }

    private static async Task WriteSseEventAsync(
        HttpResponse response,
        string eventName,
        object payload,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);

        await response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
