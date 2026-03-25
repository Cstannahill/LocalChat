namespace LocalChat.Domain.Entities.Agents;

public sealed class AgentSampleDialogue
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    public string AssistantMessage { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public Agent? Agent { get; set; }
}
