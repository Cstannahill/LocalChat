using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Chat;

public static class ConversationMessageSeeder
{
    public static bool SeedGreetingIfNeeded(Conversation conversation)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var greeting = conversation.Character?.Greeting?.Trim();
        if (string.IsNullOrWhiteSpace(greeting))
        {
            return false;
        }

        var hasSeedGreeting = conversation.Messages.Any(x => x.OriginType == MessageOriginType.SeedGreeting);
        if (hasSeedGreeting)
        {
            return false;
        }

        // Only seed automatically when the conversation is otherwise empty.
        // If messages already exist, do not retroactively insert and renumber.
        if (conversation.Messages.Count > 0)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var seededMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            OriginType = MessageOriginType.SeedGreeting,
            Content = greeting,
            SequenceNumber = 1,
            SelectedVariantIndex = 0,
            CreatedAt = now
        };

        seededMessage.Variants.Add(new MessageVariant
        {
            Id = Guid.NewGuid(),
            MessageId = seededMessage.Id,
            Content = greeting,
            VariantIndex = 0,
            CreatedAt = now,
            ProviderType = null,
            ModelIdentifier = null,
            ModelProfileId = null,
            GenerationPresetId = null,
            RuntimeSourceType = null,
            GenerationStartedAt = null,
            GenerationCompletedAt = null,
            ResponseTimeMs = null
        });

        conversation.Messages.Add(seededMessage);
        conversation.UpdatedAt = now;
        return true;
    }
}
