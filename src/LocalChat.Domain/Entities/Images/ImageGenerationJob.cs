using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Entities.Images;

public sealed class ImageGenerationJob
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public Guid ConversationId { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string PromptText { get; set; } = string.Empty;

    public string NegativePromptText { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public int Steps { get; set; }

    public double Cfg { get; set; }

    public long Seed { get; set; }

    public string? ProviderJobId { get; set; }

    public ImageGenerationJobStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Agent? Agent { get; set; }

    public Conversation? Conversation { get; set; }

    public ICollection<GeneratedImageAsset> Assets { get; set; } = new List<GeneratedImageAsset>();
}
