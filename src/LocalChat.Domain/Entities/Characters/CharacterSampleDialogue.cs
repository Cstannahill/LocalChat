namespace LocalChat.Domain.Entities.Characters;

public sealed class CharacterSampleDialogue
{
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    public string AssistantMessage { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public Character? Character { get; set; }
}
