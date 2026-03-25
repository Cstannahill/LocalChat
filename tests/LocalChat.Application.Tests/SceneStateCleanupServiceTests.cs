using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Features.Memory;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class SceneStateCleanupServiceTests
{
    [Fact]
    public async Task PruneStaleAsync_RemovesLowValueFamiliesEarlier()
    {
        var now = DateTime.UtcNow;

        var repo = new FakeMemoryRepository(
            new MemoryItem
            {
                Id = Guid.NewGuid(),
                Kind = MemoryKind.SceneState,
                Category = MemoryCategory.SceneState,
                Content = "She looks nervous.",
                SlotFamily = MemorySlotFamily.EmotionalState,
                SupersededAtSequenceNumber = 12,
                UpdatedAt = now.AddHours(-5),
                CreatedAt = now.AddHours(-5)
            },
            new MemoryItem
            {
                Id = Guid.NewGuid(),
                Kind = MemoryKind.SceneState,
                Category = MemoryCategory.SceneState,
                Content = "They are on a balcony.",
                SlotFamily = MemorySlotFamily.Location,
                UpdatedAt = now.AddHours(-5),
                CreatedAt = now.AddHours(-5)
            });

        var service = new SceneStateCleanupService(
            repo,
            new SceneStateCleanupOptions
            {
                EmotionalStateMaxAgeHours = 3,
                LocationMaxAgeHours = 36
            });

        var result = await service.PruneStaleAsync();

        Assert.Equal(2, result.ScannedCount);
        Assert.Equal(1, result.RemovedCount);
        Assert.Single(repo.Items);
        Assert.Equal(MemorySlotFamily.Location, repo.Items[0].SlotFamily);
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
