namespace LocalChat.Application.Background;

public interface IConversationBackgroundWorkScheduler
{
    Task ScheduleConversationChangeAsync(
        Guid conversationId,
        ConversationBackgroundWorkType workType,
        string reason,
        CancellationToken cancellationToken = default);
}
