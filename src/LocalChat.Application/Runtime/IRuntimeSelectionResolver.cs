using LocalChat.Domain.Entities.Characters;
using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Application.Runtime;

public interface IRuntimeSelectionResolver
{
    Task<ResolvedRuntimeSelection> ResolveAsync(
        Character character,
        Conversation conversation,
        string? oneTurnOverrideProvider,
        string? oneTurnOverrideModelIdentifier,
        CancellationToken cancellationToken = default);
}
