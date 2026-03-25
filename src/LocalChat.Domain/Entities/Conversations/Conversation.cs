using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.Personas;

namespace LocalChat.Domain.Entities.Conversations;

public sealed class Conversation
{
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public Guid? UserPersonaId { get; set; }

    public Guid? ParentConversationId { get; set; }

    public Guid? BranchedFromMessageId { get; set; }

    public Guid? RuntimeModelProfileOverrideId { get; set; }

    public Guid? RuntimeGenerationPresetOverrideId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? DirectorInstructions { get; set; }

    public DateTime? DirectorInstructionsUpdatedAt { get; set; }

    public string? SceneContext { get; set; }

    public DateTime? SceneContextUpdatedAt { get; set; }

    public bool IsOocModeEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Character? Character { get; set; }

    public UserPersona? UserPersona { get; set; }

    public ModelProfile? RuntimeModelProfileOverride { get; set; }

    public GenerationPreset? RuntimeGenerationPresetOverride { get; set; }

    public Conversation? ParentConversation { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<SummaryCheckpoint> SummaryCheckpoints { get; set; } = new List<SummaryCheckpoint>();

    public ICollection<MemoryItem> Memories { get; set; } = new List<MemoryItem>();
}
