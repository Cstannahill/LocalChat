namespace LocalChat.Application.Abstractions.ImageGeneration;

public sealed class ImageGenerationProviderResult
{
    public required string ProviderJobId { get; init; }

    public required IReadOnlyList<GeneratedImageBinary> Images { get; init; }
}
