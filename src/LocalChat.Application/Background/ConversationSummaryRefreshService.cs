using System.Text;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalChat.Application.Background;

public sealed class ConversationSummaryRefreshService : IConversationSummaryRefreshService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IInferenceProvider _inferenceProvider;
    private readonly ConversationBackgroundWorkOptions _options;
    private readonly ILogger<ConversationSummaryRefreshService> _logger;

    public ConversationSummaryRefreshService(
        IConversationRepository conversationRepository,
        IInferenceProvider inferenceProvider,
        ConversationBackgroundWorkOptions options)
        : this(
            conversationRepository,
            inferenceProvider,
            options,
            NullLogger<ConversationSummaryRefreshService>.Instance)
    {
    }

    public ConversationSummaryRefreshService(
        IConversationRepository conversationRepository,
        IInferenceProvider inferenceProvider,
        ConversationBackgroundWorkOptions options,
        ILogger<ConversationSummaryRefreshService> logger)
    {
        _conversationRepository = conversationRepository;
        _inferenceProvider = inferenceProvider;
        _options = options;
        _logger = logger;
    }

    public async Task<ConversationSummaryRefreshResult> RefreshAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        if (orderedMessages.Count < _options.SummaryMinMessagesBeforeRefresh)
        {
            return new ConversationSummaryRefreshResult
            {
                ConversationId = conversationId,
                Refreshed = false,
                Reason = $"Only {orderedMessages.Count} messages; below summary threshold."
            };
        }

        var keepRaw = Math.Max(2, _options.SummaryRecentMessagesToKeepRaw);
        var summarizableCount = orderedMessages.Count - keepRaw;

        if (summarizableCount <= 0)
        {
            return new ConversationSummaryRefreshResult
            {
                ConversationId = conversationId,
                Refreshed = false,
                Reason = "Not enough older messages to summarize."
            };
        }

        var endSequence = orderedMessages[summarizableCount - 1].SequenceNumber;

        var latestSummary = conversation.SummaryCheckpoints
            .OrderByDescending(x => x.EndSequenceNumber)
            .FirstOrDefault();

        if (latestSummary is not null)
        {
            if (latestSummary.EndSequenceNumber >= endSequence)
            {
                return new ConversationSummaryRefreshResult
                {
                    ConversationId = conversationId,
                    Refreshed = false,
                    Reason = "Existing summary already covers the summarizable range.",
                    StartSequenceNumber = latestSummary.StartSequenceNumber,
                    EndSequenceNumber = latestSummary.EndSequenceNumber,
                    SummaryText = latestSummary.SummaryText
                };
            }

            var newCoveredMessageCount = endSequence - latestSummary.EndSequenceNumber;
            if (newCoveredMessageCount < _options.SummaryMinNewMessagesSinceLastRefresh)
            {
                return new ConversationSummaryRefreshResult
                {
                    ConversationId = conversationId,
                    Refreshed = false,
                    Reason = $"Only {newCoveredMessageCount} new summarizable messages since last summary."
                };
            }
        }

        IReadOnlyList<LocalChat.Domain.Entities.Conversations.Message> messagesForRefresh;

        if (latestSummary is null)
        {
            messagesForRefresh = orderedMessages
                .Take(Math.Min(summarizableCount, _options.SummaryMaxMessagesInPrompt))
                .ToList();
        }
        else
        {
            messagesForRefresh = orderedMessages
                .Where(x =>
                    x.SequenceNumber > latestSummary.EndSequenceNumber &&
                    x.SequenceNumber <= endSequence)
                .TakeLast(_options.SummaryMaxMessagesInPrompt)
                .ToList();
        }

        if (messagesForRefresh.Count == 0)
        {
            return new ConversationSummaryRefreshResult
            {
                ConversationId = conversationId,
                Refreshed = false,
                Reason = "No new messages available for summary refresh."
            };
        }

        var prompt = BuildSummaryPrompt(
            conversation,
            latestSummary?.SummaryText,
            messagesForRefresh,
            endSequence);

        var rawSummary = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            null,
            cancellationToken);

        var summaryText = rawSummary.Trim();
        if (string.IsNullOrWhiteSpace(summaryText))
        {
            return new ConversationSummaryRefreshResult
            {
                ConversationId = conversationId,
                Refreshed = false,
                Reason = "Summary generation returned empty content."
            };
        }

        await _conversationRepository.DeleteSummaryCheckpointsAsync(
            conversationId,
            cancellationToken);

        await _conversationRepository.AddSummaryCheckpointAsync(
            new LocalChat.Domain.Entities.Conversations.SummaryCheckpoint
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                StartSequenceNumber = 1,
                EndSequenceNumber = endSequence,
                SummaryText = summaryText,
                CreatedAt = DateTime.UtcNow
            },
            cancellationToken);

        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Conversation summary refreshed. ConversationId={ConversationId}, EndSequence={EndSequence}, RefreshedMessageCount={MessageCount}",
            conversationId,
            endSequence,
            messagesForRefresh.Count);

        return new ConversationSummaryRefreshResult
        {
            ConversationId = conversationId,
            Refreshed = true,
            Reason = "Summary refreshed.",
            StartSequenceNumber = 1,
            EndSequenceNumber = endSequence,
            SummaryText = summaryText
        };
    }

    private static string BuildSummaryPrompt(
        LocalChat.Domain.Entities.Conversations.Conversation conversation,
        string? priorSummary,
        IReadOnlyList<LocalChat.Domain.Entities.Conversations.Message> messages,
        int endSequence)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Produce a concise rolling conversation summary for future prompt context.");
        sb.AppendLine("Focus on:");
        sb.AppendLine("- stable developments");
        sb.AppendLine("- relationship changes");
        sb.AppendLine("- important revealed facts");
        sb.AppendLine("- current situation continuity");
        sb.AppendLine("- unresolved questions or tension");
        sb.AppendLine();
        sb.AppendLine("Do not include bullet points unless necessary.");
        sb.AppendLine("Do not narrate outside the conversation.");
        sb.AppendLine();

        sb.AppendLine("Agent:");
        sb.AppendLine($"Name: {conversation.Agent?.Name}");
        sb.AppendLine($"Description: {conversation.Agent?.Description}");
        sb.AppendLine($"Scenario: {conversation.Agent?.Scenario}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(conversation.SceneContext))
        {
            sb.AppendLine("Scene Context:");
            sb.AppendLine(conversation.SceneContext);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(priorSummary))
        {
            sb.AppendLine("Existing Rolling Summary:");
            sb.AppendLine(priorSummary);
            sb.AppendLine();
            sb.AppendLine("Update that summary using these newer messages:");
        }
        else
        {
            sb.AppendLine("Summarize these messages:");
        }

        foreach (var message in messages)
        {
            sb.AppendLine($"{message.Role}: {message.Content}");
        }

        sb.AppendLine();
        sb.AppendLine($"The refreshed summary should represent the conversation up through sequence {endSequence}.");

        return sb.ToString();
    }
}
