namespace LocalChat.Application.Abstractions.ImageGeneration;

public sealed class ImageGenerationRequest
{
    public required string Prompt { get; init; }

    public string? NegativePrompt { get; init; }

    public int Width { get; init; } = 1024;

    public int Height { get; init; } = 1024;

    public int Steps { get; init; } = 8;

    public double Cfg { get; init; } = 1.0;

    public long Seed { get; init; } = -1;
}
