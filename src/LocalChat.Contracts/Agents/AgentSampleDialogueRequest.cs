namespace LocalChat.Contracts.Agents;

public sealed class AgentSampleDialogueRequest
{
    public required string UserMessage { get; init; }

    public required string AssistantMessage { get; init; }
}
