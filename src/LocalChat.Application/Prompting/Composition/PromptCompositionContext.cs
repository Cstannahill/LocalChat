using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Lorebooks;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Personas;

namespace LocalChat.Application.Prompting.Composition;

public sealed class PromptCompositionContext
{
    public required Character Character { get; init; }

    public required Conversation Conversation { get; init; }

    public UserPersona? UserPersona { get; init; }

    public IReadOnlyList<MemoryItem> ExplicitMemories { get; init; } = Array.Empty<MemoryItem>();

    public IReadOnlyList<LoreEntry> RelevantLoreEntries { get; init; } = Array.Empty<LoreEntry>();

    public string? RollingSummary { get; init; }

    public IReadOnlyList<Message> PriorMessages { get; init; } = Array.Empty<Message>();

    public string? CurrentUserMessage { get; init; }

    public bool ContinueWithoutUserMessage { get; init; }
}
