using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Tests;

public sealed class PromptDurableMemoryDebugTests
{
    [Fact]
    public void Compose_EmitsSelectedAndSuppressedDurableMemoryDebug()
    {
        var composer = new PromptComposer(new FakeTokenEstimator());

        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Elena",
            Greeting = "Hello.",
            PersonalityDefinition = "Warm and perceptive.",
            Scenario = "A quiet balcony.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CharacterId = character.Id,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = composer.Compose(new LocalChat.Application.Prompting.Composition.PromptCompositionContext
        {
            Character = character,
            Conversation = conversation,
            CurrentUserMessage = "Continue.",
            ExplicitMemories = Enumerable.Range(1, 40)
                .Select(i => new MemoryItem
                {
                    Id = Guid.NewGuid(),
                    Category = MemoryCategory.UserFact,
                    Kind = MemoryKind.DurableFact,
                    Content = $"Durable memory {i} " + string.Join(' ', Enumerable.Repeat("token", 12)),
                    ReviewStatus = MemoryReviewStatus.Accepted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
                })
                .ToList()
        });

        Assert.NotEmpty(result.SelectedDurableMemory);
        Assert.NotEmpty(result.SuppressedDurableMemory);
    }

    private sealed class FakeTokenEstimator : ITokenEstimator
    {
        public int EstimateTokens(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
