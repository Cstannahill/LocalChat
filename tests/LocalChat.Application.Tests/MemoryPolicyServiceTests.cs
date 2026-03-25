using LocalChat.Application.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class MemoryPolicyServiceTests
{
    private readonly IMemoryPolicyService _service = new MemoryPolicyService();

    [Fact]
    public void ResolveAutomaticScope_DefaultsToConversation()
    {
        var scope = _service.ResolveAutomaticScope();

        Assert.Equal(MemoryScopeType.Conversation, scope);
    }

    [Fact]
    public void ApplyAutomaticDefaults_SetsConversationScopeAndSequenceState()
    {
        var memory = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Kind = MemoryKind.DurableFact,
            Content = "User likes jazz."
        };

        var conversationId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        _service.ApplyAutomaticDefaults(memory, conversationId, agentId, 12);

        Assert.Equal(MemoryScopeType.Conversation, memory.ScopeType);
        Assert.Equal(conversationId, memory.ConversationId);
        Assert.Equal(agentId, memory.AgentId);
        Assert.Equal(12, memory.SourceMessageSequenceNumber);
        Assert.Equal(12, memory.LastObservedSequenceNumber);
        Assert.Null(memory.SupersededAtSequenceNumber);
        Assert.Null(memory.ExpiresAt);
    }

    [Fact]
    public void ConversationScopedMemory_GetsHigherScoreThanAgentScopedMemory()
    {
        var conversationId = Guid.NewGuid();

        var conversationScoped = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Kind = MemoryKind.DurableFact,
            ScopeType = MemoryScopeType.Conversation,
            ConversationId = conversationId,
            AgentId = Guid.NewGuid(),
            Content = "Conversation local fact."
        };

        var agentScoped = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Kind = MemoryKind.DurableFact,
            ScopeType = MemoryScopeType.Agent,
            AgentId = Guid.NewGuid(),
            Content = "Agent shared fact."
        };

        var conversationScore = _service.ApplyRetrievalPolicy(conversationScoped, 0.7, conversationId, 40);
        var agentScore = _service.ApplyRetrievalPolicy(agentScoped, 0.7, conversationId, 40);

        Assert.True(conversationScore > agentScore);
    }

    [Fact]
    public void SupersededSessionState_IsExcludedFromRetrieval()
    {
        var item = new MemoryItem
        {
            Id = Guid.NewGuid(),
            Kind = MemoryKind.SessionState,
            ScopeType = MemoryScopeType.Conversation,
            ConversationId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            Content = "Agent is wearing a red coat.",
            SupersededAtSequenceNumber = 18
        };

        var shouldExclude = _service.ShouldExcludeFromRetrieval(
            item,
            item.ConversationId!.Value,
            20);

        Assert.True(shouldExclude);
    }
}
