namespace LocalChat.Application.Abstractions.Speech;

public interface ISpeechSynthesisProvider
{
    Task<SpeechSynthesisResult> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken = default);
}
