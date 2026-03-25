namespace LocalChat.Application.Abstractions.Inference;

public interface IInferenceProvider
{
    Task<string> StreamCompletionAsync(
        string prompt,
        Func<string, CancellationToken, Task> onDelta,
        InferenceExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);
}
