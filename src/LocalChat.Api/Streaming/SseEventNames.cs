namespace LocalChat.Api.Streaming;

public static class SseEventNames
{
    public const string Started = "started";
    public const string TokenDelta = "token-delta";
    public const string Completed = "completed";
    public const string Error = "error";
}