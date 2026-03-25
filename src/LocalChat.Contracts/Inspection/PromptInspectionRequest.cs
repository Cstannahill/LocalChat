namespace LocalChat.Contracts.Inspection;

public sealed class PromptInspectionRequest
{
    public required Guid AgentId { get; init; }

    public Guid? ConversationId { get; init; }

    public Guid? UserProfileId { get; init; }

    public required string Query { get; init; }
}
