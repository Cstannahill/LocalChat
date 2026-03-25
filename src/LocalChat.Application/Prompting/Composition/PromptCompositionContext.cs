using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.UserProfiles;

namespace LocalChat.Application.Prompting.Composition;

public sealed class PromptCompositionContext
{
    public required Agent Agent { get; init; }

    public required Conversation Conversation { get; init; }

    public UserProfile? UserProfile { get; init; }

    public IReadOnlyList<MemoryItem> ExplicitMemories { get; init; } = Array.Empty<MemoryItem>();

    public IReadOnlyList<LoreEntry> RelevantLoreEntries { get; init; } = Array.Empty<LoreEntry>();

    public string? RollingSummary { get; init; }

    public IReadOnlyList<Message> PriorMessages { get; init; } = Array.Empty<Message>();

    public string? CurrentUserMessage { get; init; }

    public bool ContinueWithoutUserMessage { get; init; }
}
