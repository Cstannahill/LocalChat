using LocalChat.Application.Speech;
using LocalChat.Contracts.Tts;
using LocalChat.Domain.Entities.Audio;

namespace LocalChat.Api.Endpoints;

public static class TtsEndpoints
{
    public static IEndpointRouteBuilder MapTtsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tts")
            .WithTags("TTS");

        // Legacy compatibility for older UI payloads: { messageId, voice?, modelIdentifier?, speed? }
        group.MapPost("/synthesize", async (
            LegacySynthesizeSpeechRequest request,
            SpeechOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var clip = await orchestrator.SynthesizeMessageAsync(
                    request.MessageId,
                    request.Voice,
                    request.ModelIdentifier,
                    request.Speed,
                    cancellationToken);

                return Results.Ok(ToResponse(clip));
            }
            catch (HttpRequestException ex)
            {
                return Results.Problem(
                    detail: $"TTS provider is unreachable: {ex.Message}",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "TTS Provider Unavailable");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("was not found.", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/messages/{messageId:guid}/synthesize", async (
            Guid messageId,
            SynthesizeSpeechRequest request,
            SpeechOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var clip = await orchestrator.SynthesizeMessageAsync(
                    messageId,
                    request.Voice,
                    request.ModelIdentifier,
                    request.Speed,
                    cancellationToken);

                return Results.Ok(ToResponse(clip));
            }
            catch (HttpRequestException ex)
            {
                return Results.Problem(
                    detail: $"TTS provider is unreachable: {ex.Message}",
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "TTS Provider Unavailable");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("was not found.", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/messages/{messageId:guid}", async (
            Guid messageId,
            SpeechOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            var clips = await orchestrator.ListByMessageAsync(messageId, cancellationToken);
            return Results.Ok(clips.Select(ToResponse).ToList());
        });

        return app;
    }

    private static SpeechClipResponse ToResponse(SpeechClip x) =>
        new()
        {
            Id = x.Id,
            CharacterId = x.CharacterId,
            ConversationId = x.ConversationId,
            MessageId = x.MessageId,
            Provider = x.Provider,
            Voice = x.Voice,
            ModelIdentifier = x.ModelIdentifier,
            ResponseFormat = x.ResponseFormat,
            ContentType = x.ContentType,
            Url = x.RelativeUrl,
            SourceText = x.SourceText,
            CreatedAt = x.CreatedAt
        };

    private sealed class LegacySynthesizeSpeechRequest
    {
        public Guid MessageId { get; init; }

        public string? Voice { get; init; }

        public string? ModelIdentifier { get; init; }

        public double? Speed { get; init; }
    }
}
