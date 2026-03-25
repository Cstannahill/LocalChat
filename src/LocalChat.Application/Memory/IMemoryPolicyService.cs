using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Memory;

public interface IMemoryPolicyService
{
    MemoryScopeType ResolveAutomaticScope();

    void ApplyAutomaticDefaults(
        MemoryItem item,
        Guid conversationId,
        Guid agentId,
        int sourceMessageSequenceNumber);

    void ReinforceObservation(
        MemoryItem item,
        int observedSequenceNumber);

    void MarkSuperseded(
        MemoryItem item,
        int supersededAtSequenceNumber);

    bool ShouldExcludeFromRetrieval(
        MemoryItem item,
        Guid activeConversationId,
        int currentSequenceNumber);

    double ApplyRetrievalPolicy(
        MemoryItem item,
        double semanticScore,
        Guid activeConversationId,
        int currentSequenceNumber);
}
