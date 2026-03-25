using LocalChat.Domain.Entities.Conversations;

namespace LocalChat.Application.Features.Summaries;

public interface IConversationSummaryService
{
    Task<string> BuildRollingSummaryAsync(
        string? existingSummary,
        IReadOnlyList<Message> messagesToSummarize,
        CancellationToken cancellationToken = default);
}