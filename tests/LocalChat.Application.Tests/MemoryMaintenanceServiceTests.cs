using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Features.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class MemoryMaintenanceServiceTests
{
    [Fact]
    public async Task RebuildKeysAsync_RebuildsNormalizedSlotAndFamily()
    {
        var repo = new FakeMemoryRepository(
            new MemoryItem
            {
                Id = Guid.NewGuid(),
                Category = MemoryCategory.SceneState,
                Kind = MemoryKind.SceneState,
                Content = "The character is currently wearing a yellow sundress.",
                ReviewStatus = MemoryReviewStatus.Accepted,
                NormalizedKey = null,
                SlotKey = null,
                SlotFamily = MemorySlotFamily.None,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        var service = new MemoryMaintenanceService(
            repo,
            new MemoryProposalQualityEvaluator());

        var result = await service.RebuildKeysAsync();

        Assert.Equal(1, result.ScannedCount);
        Assert.Equal(1, result.RebuiltNormalizedKeyCount);
        Assert.Equal(1, result.RebuiltSlotKeyCount);
        Assert.Equal(1, result.RebuiltSlotFamilyCount);
        Assert.Equal(1, result.UpdatedCount);

        Assert.NotNull(repo.Items[0].NormalizedKey);
        Assert.NotNull(repo.Items[0].SlotKey);
        Assert.Equal(MemorySlotFamily.Outfit, repo.Items[0].SlotFamily);
    }

    private sealed class FakeMemoryRepository : IMemoryRepository
    {
        public FakeMemoryRepository(params MemoryItem[] items)
        {
            Items = items.ToList();
        }

        public List<MemoryItem> Items { get; }

        public Task<IReadOnlyList<MemoryItem>> ListAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items);

        public Task<MemoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<MemoryItem>> ListByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items.Where(x => x.ConversationId == conversationId).ToList());

        public Task<IReadOnlyList<MemoryItem>> ListByCharacterAsync(Guid characterId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items.Where(x => x.CharacterId == characterId).ToList());

        public Task<IReadOnlyList<MemoryItem>> ListForProposalComparisonAsync(Guid characterId, Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MemoryItem>>(Items);

        public Task<MemoryItem?> FindActiveByNormalizedKeyAsync(Guid characterId, Guid? conversationId, string normalizedKey, MemoryKind kind, CancellationToken cancellationToken = default)
            => Task.FromResult<MemoryItem?>(null);

        public Task<MemoryItem?> FindTrackedBySlotAsync(Guid characterId, Guid? conversationId, string slotKey, MemoryKind kind, CancellationToken cancellationToken = default)
            => Task.FromResult<MemoryItem?>(null);

        public Task<MemoryItem?> FindTrackedByFamilyAsync(Guid characterId, Guid? conversationId, MemorySlotFamily slotFamily, MemoryKind kind, CancellationToken cancellationToken = default)
            => Task.FromResult<MemoryItem?>(null);

        public Task<int> DeleteExpiredSceneStateAsync(DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> DeleteByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            var removed = Items.RemoveAll(x => ids.Contains(x.Id));
            return Task.FromResult(removed);
        }

        public Task<MemoryItem> AddAsync(MemoryItem memoryItem, CancellationToken cancellationToken = default)
        {
            Items.Add(memoryItem);
            return Task.FromResult(memoryItem);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
