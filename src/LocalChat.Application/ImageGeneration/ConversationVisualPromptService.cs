using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Inspection;

namespace LocalChat.Application.ImageGeneration;

public sealed class ConversationVisualPromptService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IRetrievalService _retrievalService;
    private readonly IInferenceProvider _inferenceProvider;

    public ConversationVisualPromptService(
        IConversationRepository conversationRepository,
        IRetrievalService retrievalService,
        IInferenceProvider inferenceProvider)
    {
        _conversationRepository = conversationRepository;
        _retrievalService = retrievalService;
        _inferenceProvider = inferenceProvider;
    }

    public async Task<ContextualImagePromptResult> GenerateAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, cancellationToken)
                           ?? throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");

        var orderedMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        if (orderedMessages.Count == 0)
        {
            throw new InvalidOperationException("Cannot generate an image prompt from an empty conversation.");
        }

        var retrievalQuery = string.Join(
            "\n",
            orderedMessages
                .TakeLast(8)
                .Select(x => $"{x.Role}: {x.Content}"));

        var retrieval = await _retrievalService.InspectAsync(
            conversation.CharacterId,
            conversation.Id,
            retrievalQuery,
            cancellationToken);

        var summary = conversation.SummaryCheckpoints
            .OrderByDescending(x => x.EndSequenceNumber)
            .FirstOrDefault()?.SummaryText;

        var prompt = BuildPrompt(conversation, orderedMessages, retrieval, summary);

        var raw = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            null,
            cancellationToken);

        var parsed = ParseResponse(raw);

        return new ContextualImagePromptResult
        {
            ConversationId = conversation.Id,
            PositivePrompt = parsed.PositivePrompt,
            NegativePrompt = parsed.NegativePrompt,
            SceneSummary = parsed.SceneSummary,
            AssumptionsOrUnknowns = parsed.AssumptionsOrUnknowns
        };
    }

    private static string BuildPrompt(
        Domain.Entities.Conversations.Conversation conversation,
        IReadOnlyList<Domain.Entities.Conversations.Message> orderedMessages,
        RetrievalInspectionResult retrieval,
        string? summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You generate text-to-image prompts from character-chat conversation context.");
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Your job:");
        sb.AppendLine("- identify the current visible moment or action");
        sb.AppendLine("- include durable visual details that are explicitly supported");
        sb.AppendLine("- include current clothing, pose, environment, mood, and composition if supported");
        sb.AppendLine("- do not invent visual details that were not stated");
        sb.AppendLine("- if the user's appearance is unspecified, describe them neutrally or as another person");
        sb.AppendLine("- prioritize the current moment over older context, while preserving relevant continuity");
        sb.AppendLine("- keep the positive prompt strong but not bloated");
        sb.AppendLine();
        sb.AppendLine("Return this exact JSON shape:");
        sb.AppendLine("""
{
  "positivePrompt": "string",
  "negativePrompt": "string",
  "sceneSummary": "string",
  "assumptionsOrUnknowns": ["string"]
}
""");
        sb.AppendLine();

        sb.AppendLine("Character:");
        sb.AppendLine($"Name: {conversation.Character?.Name}");
        sb.AppendLine($"Description: {conversation.Character?.Description}");
        sb.AppendLine($"Scenario: {conversation.Character?.Scenario}");
        sb.AppendLine($"Personality: {conversation.Character?.PersonalityDefinition}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(conversation.SceneContext))
        {
            sb.AppendLine("Scene Context:");
            sb.AppendLine(conversation.SceneContext);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(summary))
        {
            sb.AppendLine("Rolling Summary:");
            sb.AppendLine(summary);
            sb.AppendLine();
        }

        if (retrieval.SelectedMemories.Count > 0)
        {
            sb.AppendLine("Relevant Memory:");
            foreach (var memory in retrieval.SelectedMemories)
            {
                sb.AppendLine($"- [{memory.Category}] {memory.Content}");
            }

            sb.AppendLine();
        }

        if (retrieval.SelectedLoreEntries.Count > 0)
        {
            sb.AppendLine("Relevant Lore:");
            foreach (var lore in retrieval.SelectedLoreEntries)
            {
                sb.AppendLine($"- {lore.Title}: {lore.Content}");
            }

            sb.AppendLine();
        }

        if (conversation.Character?.SampleDialogues?.Count > 0)
        {
            sb.AppendLine("Sample Dialogue Style Hints:");
            foreach (var example in conversation.Character.SampleDialogues.OrderBy(x => x.SortOrder).Take(3))
            {
                sb.AppendLine($"User: {example.UserMessage}");
                sb.AppendLine($"Assistant: {example.AssistantMessage}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("Recent Conversation:");
        foreach (var message in orderedMessages.TakeLast(12))
        {
            sb.AppendLine($"{message.Role}: {message.Content}");
        }

        sb.AppendLine();
        sb.AppendLine("Return JSON only.");

        return sb.ToString();
    }

    private static ParsedVisualPrompt ParseResponse(string raw)
    {
        var cleaned = ExtractJsonPayload(StripCodeFences(raw));

        try
        {
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            var positivePrompt = root.TryGetProperty("positivePrompt", out var positiveProp)
                ? positiveProp.GetString()?.Trim()
                : null;

            var negativePrompt = root.TryGetProperty("negativePrompt", out var negativeProp)
                ? negativeProp.GetString()?.Trim()
                : null;

            var sceneSummary = root.TryGetProperty("sceneSummary", out var sceneProp)
                ? sceneProp.GetString()?.Trim()
                : null;

            var assumptions = new List<string>();
            if (root.TryGetProperty("assumptionsOrUnknowns", out var assumptionsProp) &&
                assumptionsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in assumptionsProp.EnumerateArray())
                {
                    var text = item.GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        assumptions.Add(text);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(positivePrompt))
            {
                throw new InvalidOperationException("Generated visual prompt did not contain a valid positivePrompt.");
            }

            if (string.IsNullOrWhiteSpace(negativePrompt))
            {
                negativePrompt = "blurry, low quality, distorted anatomy, extra limbs, duplicate subjects, bad hands, bad face, cropped, watermark, text";
            }

            return new ParsedVisualPrompt
            {
                PositivePrompt = positivePrompt,
                NegativePrompt = negativePrompt,
                SceneSummary = sceneSummary,
                AssumptionsOrUnknowns = assumptions
            };
        }
        catch (JsonException ex)
        {
            var preview = cleaned.Length <= 400 ? cleaned : cleaned[..400];
            throw new InvalidOperationException(
                $"Generated visual prompt was not valid JSON. Preview: {preview}",
                ex);
        }
    }

    private static string StripCodeFences(string raw)
    {
        var trimmed = raw.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var lines = trimmed.Split('\n').ToList();

        if (lines.Count > 0 && lines[0].StartsWith("```", StringComparison.Ordinal))
        {
            lines.RemoveAt(0);
        }

        if (lines.Count > 0 && lines[^1].StartsWith("```", StringComparison.Ordinal))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return string.Join('\n', lines).Trim();
    }

    private static string ExtractJsonPayload(string raw)
    {
        var firstBrace = raw.IndexOf('{');
        var lastBrace = raw.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return raw[firstBrace..(lastBrace + 1)];
        }

        return raw.Trim();
    }

    private sealed class ParsedVisualPrompt
    {
        public required string PositivePrompt { get; init; }

        public required string NegativePrompt { get; init; }

        public string? SceneSummary { get; init; }

        public required IReadOnlyList<string> AssumptionsOrUnknowns { get; init; }
    }
}
