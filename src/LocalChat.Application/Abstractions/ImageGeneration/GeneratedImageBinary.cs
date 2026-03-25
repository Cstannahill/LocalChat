namespace LocalChat.Application.Abstractions.ImageGeneration;

public sealed class GeneratedImageBinary
{
    public required byte[] Bytes { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }

    public required int SortOrder { get; init; }
}
