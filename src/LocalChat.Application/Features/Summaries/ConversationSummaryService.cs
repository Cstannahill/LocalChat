using System.Text;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Enums;

namespace LocalChat.Application.Features.Summaries;

public sealed class ConversationSummaryService : IConversationSummaryService
{
    private readonly IInferenceProvider _inferenceProvider;
    private readonly SummaryOptions _options;

    public ConversationSummaryService(
        IInferenceProvider inferenceProvider,
        SummaryOptions options)
    {
        _inferenceProvider = inferenceProvider;
        _options = options;
    }

    public async Task<string> BuildRollingSummaryAsync(
        string? existingSummary,
        IReadOnlyList<Message> messagesToSummarize,
        CancellationToken cancellationToken = default)
    {
        if (messagesToSummarize.Count == 0)
        {
            return existingSummary ?? string.Empty;
        }

        var prompt = BuildSummaryPrompt(existingSummary, messagesToSummarize);

        var result = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            null,
            cancellationToken);

        var normalized = NormalizeSummary(result);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Summary generation returned an empty result.");
        }

        return normalized;
    }

    private string BuildSummaryPrompt(
        string? existingSummary,
        IReadOnlyList<Message> messagesToSummarize)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are generating a rolling summary for a chat conversation.");
        sb.AppendLine("Your job is to compress older conversation history into a concise summary for future prompt context.");
        sb.AppendLine("Preserve:");
        sb.AppendLine("- user goals, requests, and preferences");
        sb.AppendLine("- assistant commitments, decisions, and promised follow-ups");
        sb.AppendLine("- important technical details, constraints, or instructions");
        sb.AppendLine("- current state of the conversation");
        sb.AppendLine("Do not include fluff.");
        sb.AppendLine($"Return plain text only, under {_options.MaxSummaryAgents} agents.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(existingSummary))
        {
            sb.AppendLine("Existing rolling summary:");
            sb.AppendLine(existingSummary);
            sb.AppendLine();
        }

        sb.AppendLine("New messages to merge into the rolling summary:");
        foreach (var message in messagesToSummarize.OrderBy(x => x.SequenceNumber))
        {
            sb.AppendLine($"{MapRole(message.Role)}: {message.Content}");
        }

        sb.AppendLine();
        sb.Append("Updated rolling summary:");

        return sb.ToString();
    }

    private string NormalizeSummary(string text)
    {
        var normalized = text.Trim();

        if (normalized.Length <= _options.MaxSummaryAgents)
        {
            return normalized;
        }

        return normalized[.._options.MaxSummaryAgents].Trim();
    }

    private static string MapRole(MessageRole role) =>
        role switch
        {
            MessageRole.System => "System",
            MessageRole.User => "User",
            MessageRole.Assistant => "Assistant",
            _ => "Unknown"
        };
}
