using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Domain.Tests;

public sealed class MemoryItemDefaultsTests
{
    [Fact]
    public void NewMemoryItem_UsesStableDefaults()
    {
        var item = new MemoryItem();

        Assert.Equal(MemoryKind.DurableFact, item.Kind);
        Assert.Equal(MemoryScopeType.Conversation, item.ScopeType);
        Assert.Equal(MemoryReviewStatus.Accepted, item.ReviewStatus);
        Assert.Equal(MemorySlotFamily.None, item.SlotFamily);
        Assert.Equal(string.Empty, item.Content);
    }
}
