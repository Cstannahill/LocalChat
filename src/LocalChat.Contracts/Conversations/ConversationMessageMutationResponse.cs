namespace LocalChat.Contracts.Conversations;

public sealed class ConversationMessageMutationResponse
{
    public required Guid ConversationId { get; init; }

    public required Guid TargetMessageId { get; init; }

    public required string Operation { get; init; }

    public required int DeletedMessageCount { get; init; }

    public required bool SummariesInvalidated { get; init; }

    public required bool RetrievalReindexed { get; init; }

    public bool AssistantRegenerated { get; init; }

    public Guid? RegeneratedAssistantMessageId { get; init; }

    public string? RegeneratedAssistantMessage { get; init; }
}
