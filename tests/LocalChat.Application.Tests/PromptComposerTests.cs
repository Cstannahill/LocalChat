using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Prompting.Composition;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Application.Tests;

public sealed class PromptComposerTests
{
    [Fact]
    public void Compose_Includes_Director_Scene_And_Ooc_Sections_When_Present()
    {
        var composer = new PromptComposer(new FakeTokenEstimator());

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test description",
            Greeting = "Hello there",
            PersonalityDefinition = "Stay sharp.",
            Scenario = "A tense engineering bay."
        };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            AgentId = agent.Id,
            Title = "Test",
            DirectorInstructions = "Keep the reply concise and tactical.",
            SceneContext = "Emergency alarms are active in a damaged station.",
            IsOocModeEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = composer.Compose(new PromptCompositionContext
        {
            Agent = agent,
            Conversation = conversation,
            PriorMessages = [],
            CurrentUserMessage = "What do you do now?"
        });

        Assert.Contains(result.Sections, x => x.Name == "Director Instructions");
        Assert.Contains(result.Sections, x => x.Name == "Scene Context");
        Assert.Contains(result.Sections, x => x.Name == "OOC Mode");
        Assert.Contains("## Director Instructions", result.Prompt);
        Assert.Contains("## Scene Context", result.Prompt);
        Assert.Contains("## OOC Mode", result.Prompt);
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
