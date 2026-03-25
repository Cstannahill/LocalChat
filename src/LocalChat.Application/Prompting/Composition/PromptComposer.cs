using System.Diagnostics;
using System.Text;
using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Prompting;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LocalChat.Application.Prompting.Composition;

public sealed class PromptComposer : IPromptComposer
{
    private const int DurableMemoryBudgetTokens = 220;
    private const int LoreBudgetTokens = 220;
    private const int SummaryBudgetTokens = 220;

    private readonly ITokenEstimator _tokenEstimator;
    private readonly ILogger<PromptComposer> _logger;

    public PromptComposer(ITokenEstimator tokenEstimator)
        : this(tokenEstimator, NullLogger<PromptComposer>.Instance)
    {
    }

    public PromptComposer(
        ITokenEstimator tokenEstimator,
        ILogger<PromptComposer> logger)
    {
        _tokenEstimator = tokenEstimator;
        _logger = logger;
    }

    public PromptCompositionResult Compose(PromptCompositionContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!context.ContinueWithoutUserMessage &&
            string.IsNullOrWhiteSpace(context.CurrentUserMessage))
        {
            throw new ArgumentException("Current user message cannot be empty.", nameof(context));
        }

        var sections = new List<PromptSection>();

        AddSection(
            sections,
            "System Rules",
            """
            You are participating in a local character chat application.
            You must follow the precedence rules below when sections conflict.

            Precedence rules:
            1. If OOC mode is enabled, respond out-of-character even if character dialogue context exists.
            2. Director instructions steer the next reply, but should not silently overwrite stable accepted facts unless explicitly framed as a change.
            3. Active scene state represents the current moment and overrides older summary/history for temporary details such as location, posture, current action, emotion, clothing, or possessions.
            4. Durable memory is the source of truth for stable facts and overrides rolling summary when they conflict.
            5. Scene context is current framing information, but newer active scene state and newer raw conversation take precedence over stale scene framing.
            6. Rolling summary is compressed older context only; it must not override newer raw messages, active scene state, or durable memory.
            7. Recent raw conversation is the source of truth for the latest turn-level details.

            Active scene-state priority when the prompt budget is tight:
            - Location
            - PoseAction
            - EmotionalState
            - Outfit
            - Possession
            - RelationshipState
            - Misc

            Additional behavior:
            - Stay in character unless OOC mode is enabled.
            - Do not repeat greeting/setup text unless directly relevant.
            - Do not repeat personality text, scenario text, or instructions verbatim.
            - Never include the literal string "{answer}" anywhere in the response.
            - Sample dialogue is style reference, not literal history.
            - In continuation mode, continue naturally from the current moment without waiting for a new user message.
            - In continuation mode, do not repeat the immediately previous assistant reply and do not reset the scene.
            """
        );

        AddSection(sections, "Character Definition", context.Character.PersonalityDefinition);

        if (!string.IsNullOrWhiteSpace(context.Character.Description))
        {
            AddSection(sections, "Character Description", context.Character.Description);
        }

        if (!string.IsNullOrWhiteSpace(context.Character.Scenario))
        {
            AddSection(sections, "Character Scenario", context.Character.Scenario);
        }

        // Greeting is seeded into conversation history as the first assistant turn.
        // Do not inject it as a separate prompt section or it will be duplicated.

        if (context.Character.SampleDialogues.Count > 0)
        {
            var sb = new StringBuilder();

            foreach (var example in context.Character.SampleDialogues.OrderBy(x => x.SortOrder))
            {
                sb.AppendLine($"User: {example.UserMessage}");
                sb.AppendLine($"Assistant: {example.AssistantMessage}");
                sb.AppendLine();
            }

            AddSection(sections, "Sample Dialogue Examples", sb.ToString().Trim());
        }

        if (context.UserPersona is not null)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(context.UserPersona.DisplayName))
            {
                sb.AppendLine($"Display Name: {context.UserPersona.DisplayName}");
            }

            if (!string.IsNullOrWhiteSpace(context.UserPersona.Description))
            {
                sb.AppendLine($"Description: {context.UserPersona.Description}");
            }

            if (!string.IsNullOrWhiteSpace(context.UserPersona.Traits))
            {
                sb.AppendLine($"Traits: {context.UserPersona.Traits}");
            }

            if (!string.IsNullOrWhiteSpace(context.UserPersona.Preferences))
            {
                sb.AppendLine($"Preferences: {context.UserPersona.Preferences}");
            }

            if (!string.IsNullOrWhiteSpace(context.UserPersona.AdditionalInstructions))
            {
                sb.AppendLine($"Additional Instructions: {context.UserPersona.AdditionalInstructions}");
            }

            AddSection(sections, "User Persona", sb.ToString().Trim());
        }

        if (context.Conversation.IsOocModeEnabled)
        {
            AddSection(
                sections,
                "OOC Mode",
                "Enabled. Respond out-of-character. Discuss, plan, explain, or coordinate instead of speaking as the in-scene character.");
        }

        if (!string.IsNullOrWhiteSpace(context.Conversation.DirectorInstructions))
        {
            AddSection(sections, "Director Instructions", context.Conversation.DirectorInstructions!);
        }

        if (!string.IsNullOrWhiteSpace(context.Conversation.SceneContext))
        {
            AddSection(sections, "Scene Context", context.Conversation.SceneContext!);
        }

        var durableMemories = context.ExplicitMemories
            .Where(x =>
                x.ReviewStatus == MemoryReviewStatus.Accepted &&
                x.Kind != MemoryKind.SceneState)
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.UpdatedAt)
            .ToList();

        var activeSceneState = context.ExplicitMemories
            .Where(x =>
                x.ReviewStatus == MemoryReviewStatus.Accepted &&
                x.Kind == MemoryKind.SceneState &&
                !x.SupersededAtSequenceNumber.HasValue)
            .OrderByDescending(x => x.UpdatedAt)
            .ToList();

        var durableSelection = SelectBudgetedDurableMemory(durableMemories);

        if (durableSelection.Selected.Count > 0)
        {
            AddSection(
                sections,
                "Durable Memory",
                string.Join(Environment.NewLine, durableSelection.Selected.Select(x => x.PromptContent)));
        }

        var sceneStateSelection = SceneStatePromptBudgeter.Select(activeSceneState, _tokenEstimator);
        if (sceneStateSelection.Selected.Count > 0)
        {
            AddSection(
                sections,
                "Active Scene State",
                string.Join(Environment.NewLine, sceneStateSelection.Selected.Select(x => x.PromptContent)));
        }

        AddBudgetedBulletSection(
            sections,
            "Relevant Lore",
            context.RelevantLoreEntries.Select(x => $"{x.Title}: {x.Content}"),
            LoreBudgetTokens);

        if (!string.IsNullOrWhiteSpace(context.RollingSummary))
        {
            var trimmedSummary = TrimTextToBudget(context.RollingSummary, SummaryBudgetTokens);

            if (!string.IsNullOrWhiteSpace(trimmedSummary))
            {
                AddSection(sections, "Rolling Summary", trimmedSummary);
            }
        }

        if (context.PriorMessages.Count > 0)
        {
            var sb = new StringBuilder();

            foreach (var message in context.PriorMessages.OrderBy(x => x.SequenceNumber))
            {
                sb.AppendLine($"{MapRole(message.Role)}: {message.Content}");
            }

            AddSection(sections, "Recent Raw Conversation", sb.ToString().Trim());
        }
        else
        {
            AddSection(sections, "Recent Raw Conversation", "(none)");
        }

        if (context.ContinueWithoutUserMessage)
        {
            AddSection(
                sections,
                "Continuation Instruction",
                "Continue the conversation naturally from the current moment without a new user message. Advance the scene or dialogue organically. Do not repeat the immediately previous assistant reply."
            );
        }
        else
        {
            AddSection(sections, "Latest User Message", context.CurrentUserMessage!);
        }

        AddSection(sections, "Assistant Prefix", "Assistant:");

        var fullPrompt = BuildFullPrompt(sections);

        stopwatch.Stop();

        _logger.LogInformation(
            "Prompt composition completed in {ElapsedMs} ms. Sections={SectionCount}, EstimatedTokens={EstimatedTokens}, DurableSelected={DurableSelected}, DurableSuppressed={DurableSuppressed}, SceneSelected={SceneSelected}, SceneSuppressed={SceneSuppressed}",
            stopwatch.ElapsedMilliseconds,
            sections.Count,
            sections.Sum(x => x.EstimatedTokens),
            durableSelection.Selected.Count,
            durableSelection.Suppressed.Count,
            sceneStateSelection.Selected.Count,
            sceneStateSelection.Suppressed.Count);

        return new PromptCompositionResult
        {
            Prompt = fullPrompt,
            Sections = sections,
            SelectedSceneState = sceneStateSelection.Selected
                .Select(x => new PromptSceneStateSelectedDebugItem
                {
                    MemoryId = x.Memory.Id,
                    SlotFamily = x.SlotFamily.ToString(),
                    SlotKey = x.Memory.SlotKey,
                    Content = x.Memory.Content,
                    PromptContent = x.PromptContent
                })
                .ToList(),
            SuppressedSceneState = sceneStateSelection.Suppressed
                .Select(x => new PromptSceneStateSuppressedDebugItem
                {
                    MemoryId = x.MemoryId,
                    SlotFamily = x.SlotFamily.ToString(),
                    Content = x.Content,
                    Reason = x.Reason
                })
                .ToList(),
            SelectedDurableMemory = durableSelection.Selected
                .Select(x => new PromptDurableMemorySelectedDebugItem
                {
                    MemoryId = x.Memory.Id,
                    Category = x.Memory.Category.ToString(),
                    Content = x.Memory.Content,
                    PromptContent = x.PromptContent
                })
                .ToList(),
            SuppressedDurableMemory = durableSelection.Suppressed
                .Select(x => new PromptDurableMemorySuppressedDebugItem
                {
                    MemoryId = x.MemoryId,
                    Category = x.Category,
                    Content = x.Content,
                    Reason = x.Reason
                })
                .ToList()
        };
    }

    private DurablePromptSelectionResult SelectBudgetedDurableMemory(IReadOnlyList<MemoryItem> durableMemories)
    {
        var selected = new List<DurablePromptSelectedItem>();
        var suppressed = new List<DurablePromptSuppressedItem>();
        var usedTokens = 0;

        foreach (var memory in durableMemories)
        {
            var line = $"- [{memory.Category}] {memory.Content}";
            var lineTokens = _tokenEstimator.EstimateTokens(line);

            if (selected.Count > 0 && usedTokens + lineTokens > DurableMemoryBudgetTokens)
            {
                suppressed.Add(new DurablePromptSuppressedItem
                {
                    MemoryId = memory.Id,
                    Category = memory.Category.ToString(),
                    Content = memory.Content,
                    Reason = "Suppressed because durable memory budget was exhausted."
                });

                continue;
            }

            selected.Add(new DurablePromptSelectedItem
            {
                Memory = memory,
                PromptContent = line
            });

            usedTokens += lineTokens;
        }

        return new DurablePromptSelectionResult
        {
            Selected = selected,
            Suppressed = suppressed
        };
    }

    private void AddSection(List<PromptSection> sections, string name, string content)
    {
        var normalized = content.Trim();

        sections.Add(new PromptSection
        {
            Name = name,
            Content = normalized,
            EstimatedTokens = _tokenEstimator.EstimateTokens(normalized)
        });
    }

    private void AddBudgetedBulletSection(
        List<PromptSection> sections,
        string name,
        IEnumerable<string> items,
        int tokenBudget)
    {
        var included = new List<string>();
        var usedTokens = 0;

        foreach (var item in items)
        {
            var trimmed = item.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            var line = $"- {trimmed}";
            var lineTokens = _tokenEstimator.EstimateTokens(line);

            if (included.Count > 0 && usedTokens + lineTokens > tokenBudget)
            {
                break;
            }

            included.Add(line);
            usedTokens += lineTokens;

            if (usedTokens >= tokenBudget)
            {
                break;
            }
        }

        if (included.Count == 0)
        {
            return;
        }

        AddSection(sections, name, string.Join(Environment.NewLine, included));
    }

    private string TrimTextToBudget(string text, int tokenBudget)
    {
        var trimmed = text.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        if (_tokenEstimator.EstimateTokens(trimmed) <= tokenBudget)
        {
            return trimmed;
        }

        var words = trimmed
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        while (words.Count > 0)
        {
            var candidate = string.Join(' ', words) + " ...";
            if (_tokenEstimator.EstimateTokens(candidate) <= tokenBudget)
            {
                return candidate;
            }

            words.RemoveAt(words.Count - 1);
        }

        return string.Empty;
    }

    private static string BuildFullPrompt(IReadOnlyList<PromptSection> sections)
    {
        var sb = new StringBuilder();

        foreach (var section in sections)
        {
            sb.AppendLine($"## {section.Name}");
            sb.AppendLine(section.Content);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string MapRole(MessageRole role) =>
        role switch
        {
            MessageRole.System => "System",
            MessageRole.User => "User",
            MessageRole.Assistant => "Assistant",
            _ => "Unknown"
        };

    private sealed class DurablePromptSelectionResult
    {
        public required IReadOnlyList<DurablePromptSelectedItem> Selected { get; init; }

        public required IReadOnlyList<DurablePromptSuppressedItem> Suppressed { get; init; }
    }

    private sealed class DurablePromptSelectedItem
    {
        public required MemoryItem Memory { get; init; }

        public required string PromptContent { get; init; }
    }

    private sealed class DurablePromptSuppressedItem
    {
        public required Guid MemoryId { get; init; }

        public required string Category { get; init; }

        public required string Content { get; init; }

        public required string Reason { get; init; }
    }
}
