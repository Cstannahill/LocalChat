using LocalChat.Api.Streaming;
using LocalChat.Application.Chat;
using LocalChat.Application.Features.Chat.SendChatMessage;
using LocalChat.Contracts.Chat;
using LocalChat.Contracts.Chat.Streaming;

namespace LocalChat.Api.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").WithTags("Chat");

        group.MapPost(
            "/send/stream",
            async (
                HttpContext httpContext,
                SendChatMessageRequest request,
                string? overrideProvider,
                string? overrideModelIdentifier,
                ChatOrchestrator orchestrator,
                SseChatStreamWriter sseWriter,
                CancellationToken cancellationToken
            ) =>
            {
                var response = httpContext.Response;

                response.StatusCode = StatusCodes.Status200OK;
                response.Headers.ContentType = "text/event-stream";
                response.Headers.CacheControl = "no-cache";
                response.Headers.Connection = "keep-alive";
                response.Headers.Append("X-Accel-Buffering", "no");

                await sseWriter.WriteStartedAsync(
                    response,
                    new ChatStreamStartedEvent
                    {
                        Type = SseEventNames.Started,
                        CharacterId = request.CharacterId,
                        ConversationId = request.ConversationId,
                        StartedAt = DateTime.UtcNow
                    },
                    cancellationToken
                );

                try
                {
                    var result = await orchestrator.SendAsync(
                        new SendChatMessageCommand
                        {
                            CharacterId = request.CharacterId,
                            ConversationId = request.ConversationId,
                            UserPersonaId = request.UserPersonaId,
                            Message = request.Message,
                        },
                        async (delta, ct) =>
                        {
                            await sseWriter.WriteDeltaAsync(response, delta, ct);
                        },
                        overrideProvider,
                        overrideModelIdentifier,
                        cancellationToken
                    );

                    await sseWriter.WriteCompletedAsync(
                        response,
                        new ChatCompletedEvent
                        {
                            Type = SseEventNames.Completed,
                            ConversationId = result.ConversationId,
                            UserMessageId = result.UserMessageId,
                            AssistantMessageId = result.AssistantMessageId,
                            AssistantMessage = result.AssistantMessage,
                            ConversationCreated = result.ConversationCreated,
                        },
                        cancellationToken
                    );
                }
                catch (OperationCanceledException) when (
                    cancellationToken.IsCancellationRequested
                    || httpContext.RequestAborted.IsCancellationRequested
                )
                {
                    // Client disconnected or request cancelled.
                }
                catch (Exception ex)
                {
                    if (!httpContext.RequestAborted.IsCancellationRequested)
                    {
                        await sseWriter.WriteErrorAsync(response, ex.Message, cancellationToken);
                    }
                }

                return Results.Empty;
            }
        );

        group.MapPost(
            "/regenerate",
            async (
                RegenerateAssistantMessageRequest request,
                string? overrideProvider,
                string? overrideModelIdentifier,
                ChatOrchestrator orchestrator,
                CancellationToken cancellationToken
            ) =>
            {
                RegenerateAssistantMessageResult result;
                try
                {
                    result = await orchestrator.RegenerateLatestAssistantMessageAsync(
                        request.ConversationId,
                        request.AssistantMessageId,
                        overrideProvider,
                        overrideModelIdentifier,
                        cancellationToken
                    );
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }

                return Results.Ok(
                    new RegenerateAssistantMessageResponse
                    {
                        ConversationId = result.ConversationId,
                        MessageId = result.MessageId,
                        AssistantMessage = result.AssistantMessage,
                        SelectedVariantIndex = result.SelectedVariantIndex,
                        VariantCount = result.VariantCount,
                    }
                );
            }
        );

        return app;
    }
}
