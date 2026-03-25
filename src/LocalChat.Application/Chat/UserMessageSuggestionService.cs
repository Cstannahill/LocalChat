using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Application.Abstractions.Retrieval;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalChat.Application.Chat;

public sealed class UserMessageSuggestionService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IRetrievalService _retrievalService;
    private readonly IInferenceProvider _inferenceProvider;
    private readonly ILogger<UserMessageSuggestionService> _logger;

    public UserMessageSuggestionService(
        IConversationRepository conversationRepository,
        IRetrievalService retrievalService,
        IInferenceProvider inferenceProvider
    )
        : this(
            conversationRepository,
            retrievalService,
            inferenceProvider,
            NullLogger<UserMessageSuggestionService>.Instance
        ) { }

    public UserMessageSuggestionService(
        IConversationRepository conversationRepository,
        IRetrievalService retrievalService,
        IInferenceProvider inferenceProvider,
        ILogger<UserMessageSuggestionService> logger
    )
    {
        _conversationRepository = conversationRepository;
        _retrievalService = retrievalService;
        _inferenceProvider = inferenceProvider;
        _logger = logger;
    }

    public async Task<SuggestedUserMessageResult> GenerateAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var totalStopwatch = Stopwatch.StartNew();

        var conversation =
            await _conversationRepository.GetByIdWithMessagesAsync(
                conversationId,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Conversation '{conversationId}' was not found."
            );

        var orderedMessages = conversation.Messages.OrderBy(x => x.SequenceNumber).ToList();

        if (orderedMessages.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot suggest a user message for an empty conversation."
            );
        }

        var latestMessage = orderedMessages[^1];
        if (latestMessage.Role != Domain.Enums.MessageRole.Assistant)
        {
            throw new InvalidOperationException(
                "Suggested user message is only available when the latest message is from the assistant."
            );
        }

        try
        {
            var retrievalQuery = string.Join(
                "\n",
                orderedMessages.TakeLast(8).Select(x => $"{x.Role}: {x.Content}")
            );

            var retrievalStopwatch = Stopwatch.StartNew();
            var retrieval = await _retrievalService.InspectAsync(
                conversation.CharacterId,
                conversation.Id,
                retrievalQuery,
                cancellationToken
            );
            retrievalStopwatch.Stop();

            var summary = conversation
                .SummaryCheckpoints.OrderByDescending(x => x.EndSequenceNumber)
                .FirstOrDefault()
                ?.SummaryText;

            var promptBuildStopwatch = Stopwatch.StartNew();
            var prompt = BuildPrompt(conversation, orderedMessages, retrieval, summary);
            promptBuildStopwatch.Stop();

            var inferenceStopwatch = Stopwatch.StartNew();
            var raw = await _inferenceProvider.StreamCompletionAsync(
                prompt,
                static (_, _) => Task.CompletedTask,
                null,
                cancellationToken
            );
            inferenceStopwatch.Stop();

            var parseStopwatch = Stopwatch.StartNew();
            var parsed = await ParseOrRecoverResponseAsync(
                raw,
                conversation.Character?.Name,
                latestMessage.Content,
                cancellationToken
            );
            parseStopwatch.Stop();

            totalStopwatch.Stop();

            _logger.LogInformation(
                "Suggested user message generation completed in {TotalMs} ms. ConversationId={ConversationId}, CharacterId={CharacterId}, MessageCount={MessageCount}, RetrievalMs={RetrievalMs}, PromptBuildMs={PromptBuildMs}, InferenceMs={InferenceMs}, ParseMs={ParseMs}, SuggestedLength={SuggestedLength}",
                totalStopwatch.ElapsedMilliseconds,
                conversation.Id,
                conversation.CharacterId,
                orderedMessages.Count,
                retrievalStopwatch.ElapsedMilliseconds,
                promptBuildStopwatch.ElapsedMilliseconds,
                inferenceStopwatch.ElapsedMilliseconds,
                parseStopwatch.ElapsedMilliseconds,
                parsed.SuggestedMessage.Length
            );

            return new SuggestedUserMessageResult
            {
                ConversationId = conversation.Id,
                SuggestedMessage = parsed.SuggestedMessage,
                Tone = parsed.Tone,
                ReasoningSummary = parsed.ReasoningSummary,
            };
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();

            _logger.LogError(
                ex,
                "Suggested user message generation failed after {ElapsedMs} ms. ConversationId={ConversationId}, CharacterId={CharacterId}, MessageCount={MessageCount}",
                totalStopwatch.ElapsedMilliseconds,
                conversation.Id,
                conversation.CharacterId,
                orderedMessages.Count
            );

            throw;
        }
    }

    private async Task<ParsedSuggestion> ParseOrRecoverResponseAsync(
        string raw,
        string? characterName,
        string latestAssistantMessage,
        CancellationToken cancellationToken
    )
    {
        if (TryParseResponse(raw, out var parsed, out _))
        {
            return await EnsureUserPerspectiveAsync(
                parsed,
                characterName,
                latestAssistantMessage,
                cancellationToken
            );
        }

        var repairPrompt = BuildRepairPrompt(raw);
        var repaired = await _inferenceProvider.StreamCompletionAsync(
            repairPrompt,
            static (_, _) => Task.CompletedTask,
            null,
            cancellationToken
        );

        if (TryParseResponse(repaired, out parsed, out _))
        {
            return await EnsureUserPerspectiveAsync(
                parsed,
                characterName,
                latestAssistantMessage,
                cancellationToken
            );
        }

        var plainRecovered =
            TryRecoverPlainTextSuggestion(raw) ?? TryRecoverPlainTextSuggestion(repaired);

        if (!string.IsNullOrWhiteSpace(plainRecovered))
        {
            return await EnsureUserPerspectiveAsync(
                new ParsedSuggestion
                {
                    SuggestedMessage = plainRecovered,
                    Tone = null,
                    ReasoningSummary = "Recovered from non-JSON model output.",
                },
                characterName,
                latestAssistantMessage,
                cancellationToken
            );
        }

        return await EnsureUserPerspectiveAsync(
            new ParsedSuggestion
            {
                SuggestedMessage = "Can you tell me more about that?",
                Tone = null,
                ReasoningSummary =
                    "Fallback suggestion used because model output could not be parsed.",
            },
            characterName,
            latestAssistantMessage,
            cancellationToken
        );
    }

    private static string BuildPrompt(
        Domain.Entities.Conversations.Conversation conversation,
        IReadOnlyList<Domain.Entities.Conversations.Message> orderedMessages,
        Application.Inspection.RetrievalInspectionResult retrieval,
        string? summary
    )
    {
        var sb = new StringBuilder();

        sb.AppendLine("You generate a natural next user reply for a character chat conversation.");
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Your goal:");
        sb.AppendLine("- suggest a plausible next message the user might send");
        sb.AppendLine("- write ONLY from the user's point of view");
        sb.AppendLine("- keep it natural, conversational, and coherent with the immediate scene");
        sb.AppendLine("- respect the current tone, relationship, and emotional context");
        sb.AppendLine("- do not write for the assistant");
        sb.AppendLine("- do not roleplay as the character");
        sb.AppendLine("- do not narrate future events outside the user's message");
        sb.AppendLine("- do not include the prefix 'User:'");
        sb.AppendLine("- keep it to one concise message, usually 1–3 sentences");
        sb.AppendLine("- if the assistant asked a question, prefer answering or responding to it");
        sb.AppendLine(
            "- if the scene is emotional or intimate, keep the reply grounded and in-character for the user"
        );
        sb.AppendLine(
            "- avoid stage-direction narration for the assistant (no writing the assistant's actions)"
        );
        sb.AppendLine();
        sb.AppendLine("Perspective examples:");
        sb.AppendLine("- Good (user POV): \"Then tell me what you really mean.\"");
        sb.AppendLine("- Bad (assistant POV): \"She leans in and whispers...\"");
        sb.AppendLine();
        sb.AppendLine("Return this exact JSON shape:");
        sb.AppendLine(
            """
            {
              "suggestedMessage": "string",
              "tone": "string",
              "reasoningSummary": "string"
            }
            """
        );
        sb.AppendLine();

        sb.AppendLine("Character:");
        sb.AppendLine($"Name: {conversation.Character?.Name}");
        sb.AppendLine($"Description: {conversation.Character?.Description}");
        sb.AppendLine($"Scenario: {conversation.Character?.Scenario}");
        sb.AppendLine($"Personality: {conversation.Character?.PersonalityDefinition}");
        sb.AppendLine();

        if (conversation.UserPersona is not null)
        {
            sb.AppendLine("User Persona:");
            sb.AppendLine($"Name: {conversation.UserPersona.Name}");
            sb.AppendLine($"Display Name: {conversation.UserPersona.DisplayName}");
            sb.AppendLine($"Description: {conversation.UserPersona.Description}");
            sb.AppendLine($"Traits: {conversation.UserPersona.Traits}");
            sb.AppendLine($"Preferences: {conversation.UserPersona.Preferences}");
            sb.AppendLine(
                $"Additional Instructions: {conversation.UserPersona.AdditionalInstructions}"
            );
            sb.AppendLine();
        }

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

        sb.AppendLine("Recent Conversation:");
        foreach (var message in orderedMessages.TakeLast(12))
        {
            sb.AppendLine($"{message.Role}: {message.Content}");
        }

        sb.AppendLine();
        sb.AppendLine("Return JSON only.");

        return sb.ToString();
    }

    private static bool TryParseResponse(
        string raw,
        out ParsedSuggestion parsed,
        out Exception? error
    )
    {
        var cleaned = ExtractJsonPayload(StripCodeFences(raw));
        parsed = default!;
        error = null;

        try
        {
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                error = new InvalidOperationException("Suggested user message output was empty.");
                return false;
            }

            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            var suggestedMessage = root.TryGetProperty("suggestedMessage", out var messageProp)
                ? messageProp.GetString()?.Trim()
                : null;

            var tone = root.TryGetProperty("tone", out var toneProp)
                ? toneProp.GetString()?.Trim()
                : null;

            var reasoningSummary = root.TryGetProperty("reasoningSummary", out var reasoningProp)
                ? reasoningProp.GetString()?.Trim()
                : null;

            if (string.IsNullOrWhiteSpace(suggestedMessage))
            {
                error = new InvalidOperationException(
                    "Suggested user message output did not contain a valid suggestedMessage."
                );
                return false;
            }

            suggestedMessage = StripSpeakerPrefix(suggestedMessage);

            if (string.IsNullOrWhiteSpace(suggestedMessage))
            {
                error = new InvalidOperationException(
                    "Suggested user message became empty after normalization."
                );
                return false;
            }

            parsed = new ParsedSuggestion
            {
                SuggestedMessage = suggestedMessage,
                Tone = tone,
                ReasoningSummary = reasoningSummary,
            };
            return true;
        }
        catch (JsonException ex)
        {
            var preview = cleaned.Length <= 400 ? cleaned : cleaned[..400];
            error = new InvalidOperationException(
                $"Suggested user message output was not valid JSON. Preview: {preview}",
                ex
            );
            return false;
        }
    }

    private static string BuildRepairPrompt(string raw)
    {
        var trimmed = raw?.Trim() ?? string.Empty;
        var excerpt = trimmed.Length > 3000 ? trimmed[..3000] : trimmed;

        return $$"""
Return valid JSON only with this exact shape:
{
  "suggestedMessage": "string",
  "tone": "string",
  "reasoningSummary": "string"
}

Rules:
- suggestedMessage must be a single plausible next user message (2-4 sentences).
- Do not include "User:" prefix.
- Must be from the USER perspective, not assistant/character perspective.
- No markdown, no extra text.

Input:
{{excerpt}}
""";
    }

    private async Task<ParsedSuggestion> EnsureUserPerspectiveAsync(
        ParsedSuggestion parsed,
        string? characterName,
        string latestAssistantMessage,
        CancellationToken cancellationToken
    )
    {
        var normalizedMessage = NormalizeSuggestedMessage(parsed.SuggestedMessage, characterName);

        if (!IsLikelyAssistantPerspective(normalizedMessage, characterName, latestAssistantMessage))
        {
            return new ParsedSuggestion
            {
                SuggestedMessage = normalizedMessage,
                Tone = parsed.Tone,
                ReasoningSummary = parsed.ReasoningSummary,
            };
        }

        var correctionPrompt = BuildPerspectiveCorrectionPrompt(
            normalizedMessage,
            characterName,
            latestAssistantMessage
        );

        var correctedRaw = await _inferenceProvider.StreamCompletionAsync(
            correctionPrompt,
            static (_, _) => Task.CompletedTask,
            null,
            cancellationToken
        );

        var correctedText = TryRecoverPlainTextSuggestion(correctedRaw);
        if (string.IsNullOrWhiteSpace(correctedText))
        {
            correctedText = normalizedMessage;
        }

        correctedText = NormalizeSuggestedMessage(correctedText, characterName);
        if (IsLikelyAssistantPerspective(correctedText, characterName, latestAssistantMessage))
        {
            correctedText = "Can you tell me more about that?";
        }

        return new ParsedSuggestion
        {
            SuggestedMessage = correctedText,
            Tone = parsed.Tone,
            ReasoningSummary = parsed.ReasoningSummary,
        };
    }

    private static string BuildPerspectiveCorrectionPrompt(
        string candidateMessage,
        string? characterName,
        string latestAssistantMessage
    )
    {
        return $$"""
Rewrite the message so it is clearly the USER's next reply in chat.
Return plain text only (no JSON, no markdown, no labels), 2-4 sentences.

Rules:
- Must be from user perspective.
- Must NOT speak as the assistant/character.
- Must NOT include "User:" or "Assistant:".

Character name (assistant): {{characterName ?? "Unknown"}}
Latest assistant message:
{{latestAssistantMessage}}

Candidate message:
{{candidateMessage}}
""";
    }

    private static bool IsLikelyAssistantPerspective(
        string message,
        string? characterName,
        string latestAssistantMessage
    )
    {
        var trimmed = message.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return true;
        }

        if (trimmed.StartsWith("Assistant:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (
            !string.IsNullOrWhiteSpace(characterName)
            && trimmed.StartsWith($"{characterName}:", StringComparison.OrdinalIgnoreCase)
        )
        {
            return true;
        }

        if (ComputeTokenOverlap(trimmed, latestAssistantMessage) >= 0.72)
        {
            return true;
        }

        return false;
    }

    private static string NormalizeSuggestedMessage(string message, string? characterName)
    {
        var normalized = StripSpeakerPrefix(message);

        if (
            !string.IsNullOrWhiteSpace(characterName)
            && normalized.StartsWith($"{characterName}:", StringComparison.OrdinalIgnoreCase)
        )
        {
            normalized = normalized[(characterName.Length + 1)..].Trim();
        }

        return normalized.Trim();
    }

    private static double ComputeTokenOverlap(string a, string b)
    {
        var left = Tokenize(a);
        var right = Tokenize(b);
        if (left.Count == 0 || right.Count == 0)
        {
            return 0.0;
        }

        var intersection = left.Intersect(right).Count();
        var union = left.Union(right).Count();
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private static HashSet<string> Tokenize(string input)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sb = new StringBuilder();

        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
                sb.Clear();
            }
        }

        if (sb.Length > 0)
        {
            tokens.Add(sb.ToString());
        }

        return tokens;
    }

    private static string? TryRecoverPlainTextSuggestion(string raw)
    {
        var cleaned = StripSpeakerPrefix(StripCodeFences(raw ?? string.Empty).Trim());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        var lines = cleaned
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !x.StartsWith("{", StringComparison.Ordinal))
            .Where(x => !x.StartsWith("```", StringComparison.Ordinal))
            .ToList();

        if (lines.Count == 0)
        {
            return null;
        }

        var candidate = lines[0].Trim();
        return string.IsNullOrWhiteSpace(candidate) ? null : candidate;
    }

    private static string StripSpeakerPrefix(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.StartsWith("User:", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed["User:".Length..].Trim();
        }

        if (trimmed.StartsWith("Assistant:", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed["Assistant:".Length..].Trim();
        }

        return trimmed;
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

    private sealed class ParsedSuggestion
    {
        public required string SuggestedMessage { get; init; }

        public string? Tone { get; init; }

        public string? ReasoningSummary { get; init; }
    }
}
