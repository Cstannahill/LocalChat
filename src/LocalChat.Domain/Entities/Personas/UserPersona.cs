using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Domain.Entities.Personas;

public sealed class UserPersona
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Traits { get; set; } = string.Empty;

    public string Preferences { get; set; } = string.Empty;

    public string AdditionalInstructions { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
