namespace LocalChat.Application.Abstractions.ImageGeneration;

public interface IImageGenerationProvider
{
    Task<ImageGenerationProviderResult> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}
