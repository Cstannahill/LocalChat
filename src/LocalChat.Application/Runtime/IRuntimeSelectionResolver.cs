using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Application.Runtime;

public interface IRuntimeSelectionResolver
{
    Task<ResolvedRuntimeSelection> ResolveAsync(
        Agent agent,
        Conversation conversation,
        string? oneTurnOverrideProvider,
        string? oneTurnOverrideModelIdentifier,
        CancellationToken cancellationToken = default);
}
