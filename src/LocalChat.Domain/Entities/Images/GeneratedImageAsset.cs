namespace LocalChat.Domain.Entities.Images;

public sealed class GeneratedImageAsset
{
    public Guid Id { get; set; }

    public Guid ImageGenerationJobId { get; set; }

    public string RelativeUrl { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public ImageGenerationJob? ImageGenerationJob { get; set; }
}
