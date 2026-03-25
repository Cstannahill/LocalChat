namespace LocalChat.Contracts.KnowledgeBases;

public sealed class CreateKnowledgeBaseRequest
{
    public Guid? AgentId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }
}
