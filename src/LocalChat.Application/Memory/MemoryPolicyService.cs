using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Memory;

public sealed class MemoryPolicyService : IMemoryPolicyService
{
    public MemoryScopeType ResolveAutomaticScope()
    {
        return MemoryScopeType.Conversation;
    }

    public void ApplyAutomaticDefaults(
        MemoryItem item,
        Guid conversationId,
        Guid agentId,
        int sourceMessageSequenceNumber)
    {
        item.ScopeType = ResolveAutomaticScope();
        item.ConversationId = conversationId;
        item.AgentId = agentId;
        item.SourceMessageSequenceNumber = sourceMessageSequenceNumber;
        item.LastObservedSequenceNumber = sourceMessageSequenceNumber;
        item.SupersededAtSequenceNumber = null;
        item.ExpiresAt = null;
    }

    public void ReinforceObservation(
        MemoryItem item,
        int observedSequenceNumber)
    {
        var current = item.LastObservedSequenceNumber ?? item.SourceMessageSequenceNumber ?? 0;
        item.LastObservedSequenceNumber = Math.Max(current, observedSequenceNumber);
        item.ExpiresAt = null;
    }

    public void MarkSuperseded(
        MemoryItem item,
        int supersededAtSequenceNumber)
    {
        var current = item.SupersededAtSequenceNumber ?? 0;
        item.SupersededAtSequenceNumber = Math.Max(current, supersededAtSequenceNumber);
        item.ExpiresAt = null;
    }

    public bool ShouldExcludeFromRetrieval(
        MemoryItem item,
        Guid activeConversationId,
        int currentSequenceNumber)
    {
        if (item.Kind != MemoryKind.SessionState)
        {
            return false;
        }

        return item.SupersededAtSequenceNumber.HasValue;
    }

    public double ApplyRetrievalPolicy(
        MemoryItem item,
        double semanticScore,
        Guid activeConversationId,
        int currentSequenceNumber)
    {
        var score = semanticScore;

        if (item.ScopeType == MemoryScopeType.Conversation &&
            item.ConversationId.HasValue &&
            item.ConversationId.Value == activeConversationId)
        {
            score += 0.22;
        }
        else if (item.ScopeType == MemoryScopeType.Agent)
        {
            score += 0.03;
        }

        if (item.Kind != MemoryKind.SessionState)
        {
            return score;
        }

        var observedAt = item.LastObservedSequenceNumber
                         ?? item.SourceMessageSequenceNumber
                         ?? currentSequenceNumber;
        var turnsSinceObserved = Math.Max(0, currentSequenceNumber - observedAt);
        var turnWindow = GetSessionStateTurnWindow(item.SlotFamily);
        score -= ComputeTurnDistancePenalty(turnsSinceObserved, turnWindow);

        return score;
    }

    private static int GetSessionStateTurnWindow(MemorySlotFamily family)
    {
        return family switch
        {
            MemorySlotFamily.Outfit => 40,
            MemorySlotFamily.Location => 30,
            MemorySlotFamily.PoseAction => 12,
            MemorySlotFamily.EmotionalState => 6,
            _ => 15
        };
    }

    private static double ComputeTurnDistancePenalty(int turnsSinceObserved, int turnWindow)
    {
        if (turnsSinceObserved <= 0 || turnWindow <= 0)
        {
            return 0;
        }

        if (turnsSinceObserved <= turnWindow)
        {
            return 0;
        }

        var over = turnsSinceObserved - turnWindow;

        if (over <= 5)
        {
            return 0.08;
        }

        if (over <= 10)
        {
            return 0.18;
        }

        if (over <= 20)
        {
            return 0.32;
        }

        return 0.5;
    }
}
