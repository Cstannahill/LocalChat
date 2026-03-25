using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;

namespace LocalChat.Domain.Entities.Agents;

public sealed class Agent
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Greeting { get; set; } = string.Empty;

    public string PersonalityDefinition { get; set; } = string.Empty;

    public string Scenario { get; set; } = string.Empty;

    public Guid? DefaultModelProfileId { get; set; }

    public Guid? DefaultGenerationPresetId { get; set; }

    public string? DefaultTtsVoice { get; set; }

    public string? DefaultVisualStylePreset { get; set; }

    public string? DefaultVisualPromptPrefix { get; set; }

    public string? DefaultVisualNegativePrompt { get; set; }

    public string? ImagePath { get; set; }

    public DateTime? ImageUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ModelProfile? DefaultModelProfile { get; set; }

    public GenerationPreset? DefaultGenerationPreset { get; set; }

    public ICollection<AgentSampleDialogue> SampleDialogues { get; set; } = new List<AgentSampleDialogue>();

    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public ICollection<MemoryItem> Memories { get; set; } = new List<MemoryItem>();
}
