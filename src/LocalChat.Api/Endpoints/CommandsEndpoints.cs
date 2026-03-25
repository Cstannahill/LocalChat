using LocalChat.Application.Features.Commands;
using LocalChat.Contracts.Commands;

namespace LocalChat.Api.Endpoints;

public static class CommandsEndpoints
{
    public static IEndpointRouteBuilder MapCommandsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/commands")
            .WithTags("Commands");

        group.MapPost("/execute", async (
            ExecuteCommandRequest request,
            CommandOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.CommandText))
            {
                return Results.BadRequest(new { error = "Command text cannot be empty." });
            }

            var result = await orchestrator.ExecuteAsync(
                request.ConversationId,
                request.CommandText,
                cancellationToken);

            return Results.Ok(new ExecuteCommandResponse
            {
                Succeeded = result.Succeeded,
                CommandName = result.CommandName,
                Message = result.Message,
                ConversationId = result.ConversationId,
                ReloadConversation = result.ReloadConversation,
                DirectorInstructions = result.DirectorInstructions,
                SceneContext = result.SceneContext,
                IsOocModeEnabled = result.IsOocModeEnabled
            });
        });

        return app;
    }
}
