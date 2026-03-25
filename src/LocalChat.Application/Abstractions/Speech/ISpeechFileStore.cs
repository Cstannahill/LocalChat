namespace LocalChat.Application.Abstractions.Speech;

public interface ISpeechFileStore
{
    Task<string> SaveAsync(
        byte[] audioBytes,
        string responseFormat,
        CancellationToken cancellationToken = default);
}
