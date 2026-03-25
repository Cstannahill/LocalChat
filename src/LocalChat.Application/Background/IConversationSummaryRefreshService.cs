namespace LocalChat.Application.Background;

public interface IConversationSummaryRefreshService
{
    Task<ConversationSummaryRefreshResult> RefreshAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
