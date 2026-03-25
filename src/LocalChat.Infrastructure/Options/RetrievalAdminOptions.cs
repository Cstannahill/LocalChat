namespace LocalChat.Infrastructure.Options;

public sealed class RetrievalAdminOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434/api/";

    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    public int BatchSize { get; set; } = 32;

    public int TimeoutSeconds { get; set; } = 120;
}
