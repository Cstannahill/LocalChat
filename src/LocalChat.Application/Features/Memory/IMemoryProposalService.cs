namespace LocalChat.Application.Features.Memory;

public interface IMemoryProposalService
{
    Task<MemoryProposalGenerationResult> GenerateForConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
