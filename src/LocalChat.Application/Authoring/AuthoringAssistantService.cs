using System.Text;
using System.Text.Json;
using LocalChat.Application.Abstractions.Inference;

namespace LocalChat.Application.Authoring;

public sealed class AuthoringAssistantService : IAuthoringAssistantService
{
    private readonly IInferenceProvider _inferenceProvider;

    private static readonly IReadOnlyList<AuthoringFieldTemplate> Templates =
    [
        new()
        {
            EntityType = "agent",
            FieldName = "description",
            Title = "Concise Agent Overview",
            Summary = "Short, stable description for who the agent is.",
            Content =
                "A poised, observant woman with a calm presence and a habit of speaking with measured precision. She carries herself with quiet confidence and rarely wastes words.",
        },
        new()
        {
            EntityType = "agent",
            FieldName = "personalityDefinition",
            Title = "Personality Core",
            Summary = "Stable traits, style, values, strengths, and flaws.",
            Content =
                "Composed, emotionally perceptive, and quietly intense. She is patient, difficult to rattle, and naturally protective, but she can also be controlling when she feels vulnerable.",
        },
        new()
        {
            EntityType = "agent",
            FieldName = "scenario",
            Title = "Relationship + Situation",
            Summary = "Explains the current setup and dynamic with the user.",
            Content =
                "The agent and the user are alone late at night on a private balcony after a tense social event. The atmosphere is intimate, quiet, and emotionally charged.",
        },
        new()
        {
            EntityType = "agent",
            FieldName = "greeting",
            Title = "Immersive Opening",
            Summary = "Strong first message that establishes tone immediately.",
            Content =
                "*She leans against the balcony railing, glancing at you from the corner of her eye before speaking in a low, steady voice.*\n\n\"You’ve been quiet all evening. Are you finally ready to tell me what’s actually on your mind?\"",
        },
        new()
        {
            EntityType = "userProfile",
            FieldName = "description",
            Title = "User Profile Snapshot",
            Summary = "Who the user is in this roleplay or interaction.",
            Content =
                "The user is thoughtful, reserved at first, and more expressive once they feel safe. They respond well to emotional honesty and subtle intimacy.",
        },
        new()
        {
            EntityType = "userProfile",
            FieldName = "traits",
            Title = "UserProfile Traits",
            Summary = "Stable behavioral traits, not temporary moods.",
            Content =
                "Analytical, curious, quietly affectionate, patient, and occasionally stubborn.",
        },
        new()
        {
            EntityType = "userProfile",
            FieldName = "preferences",
            Title = "Interaction Preferences",
            Summary = "Useful guidance for tone and pacing.",
            Content =
                "Prefers emotionally grounded dialogue, gradual escalation, consistent continuity, and responses that feel natural rather than overly theatrical.",
        },
        new()
        {
            EntityType = "userProfile",
            FieldName = "additionalInstructions",
            Title = "Additional Guidance",
            Summary = "Extra constraints or preferences for how the agent should respond.",
            Content =
                "Keep responses immersive and emotionally intelligent. Avoid repetitive phrasing. Preserve continuity across the scene and relationship dynamic.",
        },
    ];

    private static readonly IReadOnlyList<AuthoringStarterPack> StarterPacks =
    [
        new()
        {
            Id = "romance",
            Title = "Romance",
            Summary = "Slow-burn emotional intimacy with elegant, immersive tone.",
            Concept =
                "A poised, emotionally intense romantic interest who hides vulnerability behind control and composure.",
            Vibe = "intimate, elegant, slow-burn, emotionally charged",
            Relationship = "mutual attraction with unresolved tension",
            Setting = "private balcony after a formal event",
        },
        new()
        {
            Id = "fantasy",
            Title = "Fantasy",
            Summary = "Noble courts, magical tension, high-stakes intimacy.",
            Concept =
                "A powerful court mage or noble protector balancing authority, secrets, and desire.",
            Vibe = "regal, mysterious, magical, tense",
            Relationship = "trusted ally with growing emotional dependence",
            Setting = "moonlit castle corridor or royal chamber",
        },
        new()
        {
            Id = "cyberpunk",
            Title = "Cyberpunk",
            Summary = "Stylish danger, neon atmosphere, morally gray connection.",
            Concept =
                "A sharp, capable fixer who navigates danger with charm, precision, and hidden emotional depth.",
            Vibe = "neon, dangerous, stylish, cynical but intimate",
            Relationship = "reluctant partner who gradually becomes protective",
            Setting = "rain-soaked rooftop overlooking a neon city",
        },
        new()
        {
            Id = "cozy",
            Title = "Cozy",
            Summary = "Warm, safe, emotionally grounded companionship.",
            Concept =
                "A gentle, attentive companion whose presence feels steady, comforting, and quietly affectionate.",
            Vibe = "warm, soft, comforting, domestic",
            Relationship = "close friend or partner with deep trust",
            Setting = "quiet apartment, bookstore, café, or rainy cabin evening",
        },
        new()
        {
            Id = "horror",
            Title = "Horror",
            Summary = "Unease, obsession, danger, and emotional instability.",
            Concept =
                "A compelling but unsettling figure whose affection and danger are impossible to separate.",
            Vibe = "dark, obsessive, eerie, intimate, predatory",
            Relationship = "dangerous fixation or forced alliance",
            Setting = "isolated manor, forest road, or candlelit room at night",
        },
        new()
        {
            Id = "rival",
            Title = "Rival / Enemies-to-Lovers",
            Summary = "Sharp dialogue, friction, chemistry, mutual challenge.",
            Concept =
                "A brilliant, competitive rival who masks attraction behind sarcasm, pressure, and relentless pushback.",
            Vibe = "charged, sharp, witty, competitive, romantic tension",
            Relationship = "rivals with undeniable chemistry",
            Setting = "after a confrontation, competition, or public clash",
        },
        new()
        {
            Id = "mentor",
            Title = "Mentor / Guardian",
            Summary = "Protective, competent, calm authority with emotional depth.",
            Concept =
                "A capable mentor or guardian who guides, protects, and quietly struggles with emotional attachment.",
            Vibe = "protective, grounded, wise, restrained, emotionally layered",
            Relationship = "mentor-protégé or guardian dynamic with deep trust",
            Setting = "safehouse, training room, or after surviving a dangerous event",
        },
    ];

    public AuthoringAssistantService(IInferenceProvider inferenceProvider)
    {
        _inferenceProvider = inferenceProvider;
    }

    public Task<IReadOnlyList<AuthoringFieldTemplate>> GetTemplatesAsync(
        string entityType,
        string? fieldName = null,
        CancellationToken cancellationToken = default
    )
    {
        var results = Templates
            .Where(x => string.Equals(x.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
            .Where(x =>
                string.IsNullOrWhiteSpace(fieldName)
                || string.Equals(x.FieldName, fieldName, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(x => x.FieldName)
            .ThenBy(x => x.Title)
            .ToList();

        return Task.FromResult<IReadOnlyList<AuthoringFieldTemplate>>(results);
    }

    public Task<IReadOnlyList<AuthoringStarterPack>> GetStarterPacksAsync(
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(StarterPacks);
    }

    public async Task<AuthoringEnhancementResult> EnhanceAsync(
        AuthoringEnhancementInput input,
        CancellationToken cancellationToken = default
    )
    {
        var prompt = BuildEnhancementPrompt(input);

        var raw = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            BuildExecutionSettings(input.ModelOverride),
            cancellationToken
        );

        var parsed = ParseEnhancement(raw);

        return new AuthoringEnhancementResult
        {
            EntityType = input.EntityType,
            FieldName = input.FieldName,
            Mode = input.Mode,
            OriginalText = input.CurrentText ?? string.Empty,
            SuggestedText = parsed.SuggestedText,
            Rationale = parsed.Rationale,
        };
    }

    public async Task<FullAuthoringBundleGenerationResult> GenerateFullBundleFromBriefAsync(
        FullAuthoringBundleGenerationInput input,
        CancellationToken cancellationToken = default
    )
    {
        var prompt = BuildFullBundlePrompt(input);

        var raw = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            BuildExecutionSettings(input.ModelOverride),
            cancellationToken
        );

        return ParseFullBundle(raw);
    }

    public async Task<AuthoringConsistencyCheckResult> CheckConsistencyAsync(
        AuthoringConsistencyCheckInput input,
        CancellationToken cancellationToken = default
    )
    {
        var prompt = BuildConsistencyPrompt(input);

        var raw = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            BuildExecutionSettings(input.ModelOverride),
            cancellationToken
        );

        var parsed = ParseConsistency(raw, input.EntityType);

        return parsed;
    }

    public async Task<AuthoringEnhancementResult> RepairConsistencyIssueAsync(
        ConsistencyIssueRepairInput input,
        CancellationToken cancellationToken = default
    )
    {
        var prompt = BuildIssueRepairPrompt(input);

        var raw = await _inferenceProvider.StreamCompletionAsync(
            prompt,
            static (_, _) => Task.CompletedTask,
            BuildExecutionSettings(input.ModelOverride),
            cancellationToken
        );

        var parsed = ParseEnhancement(raw);

        return new AuthoringEnhancementResult
        {
            EntityType = input.EntityType,
            FieldName = input.FieldName,
            Mode = "repair",
            OriginalText = input.CurrentText ?? string.Empty,
            SuggestedText = parsed.SuggestedText,
            Rationale = parsed.Rationale,
        };
    }

    private static InferenceExecutionSettings? BuildExecutionSettings(string? modelOverride)
    {
        if (string.IsNullOrWhiteSpace(modelOverride))
        {
            return null;
        }

        return new InferenceExecutionSettings { ModelIdentifier = modelOverride.Trim() };
    }

    private static string BuildEnhancementPrompt(AuthoringEnhancementInput input)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            "You are helping improve structured authoring fields for a local agent chat application."
        );
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Important rules:");
        sb.AppendLine("- Improve structure, clarity, consistency, and prompt efficiency.");
        sb.AppendLine("- Use surrounding field context when useful.");
        sb.AppendLine(
            "- Do not invent specific facts that are not supported by the provided text/context."
        );
        sb.AppendLine("- Do not repeat the same information across fields unnecessarily.");
        sb.AppendLine("- Keep the result appropriate for the requested field only.");
        sb.AppendLine("- Do not add markdown code fences.");
        sb.AppendLine();

        sb.AppendLine($"EntityType: {input.EntityType}");
        sb.AppendLine($"FieldName: {input.FieldName}");
        sb.AppendLine($"Mode: {input.Mode}");
        sb.AppendLine();

        sb.AppendLine("Field guidance:");
        sb.AppendLine(BuildFieldGuidance(input.EntityType, input.FieldName));
        sb.AppendLine();

        sb.AppendLine("Current field text:");
        sb.AppendLine(string.IsNullOrWhiteSpace(input.CurrentText) ? "(empty)" : input.CurrentText);
        sb.AppendLine();

        sb.AppendLine("Other field context:");
        if (input.Context.Count == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (
                var kvp in input
                    .Context.Where(x =>
                        !string.Equals(x.Key, input.FieldName, StringComparison.OrdinalIgnoreCase)
                    )
                    .OrderBy(x => x.Key)
            )
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    continue;
                }

                sb.AppendLine($"[{kvp.Key}] {kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Return JSON in exactly this shape:");
        sb.AppendLine(
            """
{
  "suggestedText": "string",
  "rationale": "short string"
}
"""
        );

        return sb.ToString();
    }

    private static string BuildFullBundlePrompt(FullAuthoringBundleGenerationInput input)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            "You are generating a full structured authoring bundle for a local agent chat application."
        );
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Goals:");
        sb.AppendLine("- Generate both agent and userProfile together from one concept brief.");
        sb.AppendLine("- Keep fields distinct and non-redundant.");
        sb.AppendLine("- AgentDescription = stable overview.");
        sb.AppendLine(
            "- AgentPersonalityDefinition = stable personality, habits, values, strengths, flaws, speech tendencies."
        );
        sb.AppendLine("- AgentScenario = relationship + current setup with the user.");
        sb.AppendLine("- AgentGreeting = immersive opening message.");
        sb.AppendLine(
            "- UserProfileDescription = useful snapshot of the user role in this interaction."
        );
        sb.AppendLine("- UserProfileTraits = stable user tendencies.");
        sb.AppendLine(
            "- UserProfilePreferences = interaction preferences that improve response quality."
        );
        sb.AppendLine(
            "- UserProfileAdditionalInstructions = extra useful guidance not already covered."
        );
        sb.AppendLine("- Make the fields coherent with each other.");
        sb.AppendLine("- Keep them useful for prompting, not bloated.");
        sb.AppendLine("- Do not add code fences.");
        sb.AppendLine();

        sb.AppendLine("Concept:");
        sb.AppendLine(string.IsNullOrWhiteSpace(input.Concept) ? "(none)" : input.Concept);
        sb.AppendLine();

        sb.AppendLine("Vibe:");
        sb.AppendLine(string.IsNullOrWhiteSpace(input.Vibe) ? "(none)" : input.Vibe);
        sb.AppendLine();

        sb.AppendLine("Relationship:");
        sb.AppendLine(
            string.IsNullOrWhiteSpace(input.Relationship) ? "(none)" : input.Relationship
        );
        sb.AppendLine();

        sb.AppendLine("Setting:");
        sb.AppendLine(string.IsNullOrWhiteSpace(input.Setting) ? "(none)" : input.Setting);
        sb.AppendLine();

        sb.AppendLine("Existing partial context:");
        if (input.ExistingContext.Count == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var kvp in input.ExistingContext.OrderBy(x => x.Key))
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    continue;
                }

                sb.AppendLine($"[{kvp.Key}] {kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Return JSON in exactly this shape:");
        sb.AppendLine(
            """
{
  "agentName": "string",
  "agentDescription": "string",
  "agentPersonalityDefinition": "string",
  "agentScenario": "string",
  "agentGreeting": "string",
  "userProfileDisplayName": "string",
  "userProfileDescription": "string",
  "userProfileTraits": "string",
  "userProfilePreferences": "string",
  "userProfileAdditionalInstructions": "string",
  "rationale": "short string"
}
"""
        );

        return sb.ToString();
    }

    private static string BuildConsistencyPrompt(AuthoringConsistencyCheckInput input)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            "You are checking structured authoring fields for consistency in a local agent chat application."
        );
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Goals:");
        sb.AppendLine("- Detect contradiction between fields.");
        sb.AppendLine("- Detect redundancy / repeated information.");
        sb.AppendLine("- Detect misplaced information that belongs in another field.");
        sb.AppendLine(
            "- Detect weak alignment between agent fields and userProfile fields when userProfile is present."
        );
        sb.AppendLine("- Prefer practical issues over nitpicks.");
        sb.AppendLine("- fieldName values in issues must use the exact field keys provided below.");
        sb.AppendLine("- Do not add code fences.");
        sb.AppendLine();

        sb.AppendLine($"EntityType: {input.EntityType}");
        sb.AppendLine();

        sb.AppendLine("Fields:");
        foreach (var kvp in input.Fields.OrderBy(x => x.Key))
        {
            sb.AppendLine($"[{kvp.Key}]");
            sb.AppendLine(string.IsNullOrWhiteSpace(kvp.Value) ? "(empty)" : kvp.Value);
            sb.AppendLine();
        }

        sb.AppendLine("Return JSON in exactly this shape:");
        sb.AppendLine(
            """
{
  "summary": "string",
  "issues": [
    {
      "severity": "info | warning | error",
      "fieldName": "exact field key from the provided input",
      "issueType": "contradiction | redundancy | misplaced | mismatch | weak",
      "description": "string",
      "suggestion": "string"
    }
  ]
}
"""
        );

        return sb.ToString();
    }

    private static string BuildIssueRepairPrompt(ConsistencyIssueRepairInput input)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            "You are repairing one specific structured authoring field in a local agent chat application."
        );
        sb.AppendLine("Return JSON only.");
        sb.AppendLine();
        sb.AppendLine("Important rules:");
        sb.AppendLine("- Repair only the target field.");
        sb.AppendLine("- Resolve the issue described.");
        sb.AppendLine("- Use other fields for context.");
        sb.AppendLine("- Do not repeat information unnecessarily.");
        sb.AppendLine("- Keep the revised field prompt-efficient.");
        sb.AppendLine("- Do not add code fences.");
        sb.AppendLine();

        sb.AppendLine($"EntityType: {input.EntityType}");
        sb.AppendLine($"FieldName: {input.FieldName}");
        sb.AppendLine($"IssueType: {input.IssueType}");
        sb.AppendLine($"IssueDescription: {input.IssueDescription}");

        if (!string.IsNullOrWhiteSpace(input.SuggestedFixHint))
        {
            sb.AppendLine($"SuggestedFixHint: {input.SuggestedFixHint}");
        }

        sb.AppendLine();
        sb.AppendLine("Field guidance:");
        sb.AppendLine(BuildRepairGuidance(input.FieldName));
        sb.AppendLine();

        sb.AppendLine("Current target field text:");
        sb.AppendLine(string.IsNullOrWhiteSpace(input.CurrentText) ? "(empty)" : input.CurrentText);
        sb.AppendLine();

        sb.AppendLine("Other field context:");
        if (input.Context.Count == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (
                var kvp in input
                    .Context.Where(x =>
                        !string.Equals(x.Key, input.FieldName, StringComparison.OrdinalIgnoreCase)
                    )
                    .OrderBy(x => x.Key)
            )
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    continue;
                }

                sb.AppendLine($"[{kvp.Key}] {kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Return JSON in exactly this shape:");
        sb.AppendLine(
            """
{
  "suggestedText": "string",
  "rationale": "short string"
}
"""
        );

        return sb.ToString();
    }

    private static string BuildFieldGuidance(string entityType, string fieldName)
    {
        var key = $"{entityType}:{fieldName}".ToLowerInvariant();

        return key switch
        {
            "agent:description" =>
                "Describe who the agent is at a stable baseline. Keep it concise. Do not dump temporary scene details here.",
            "agent:personalitydefinition" =>
                "Focus on stable personality, values, habits, speech style, strengths, weaknesses, and behavioral tendencies.",
            "agent:scenario" =>
                "Describe the current setup, relationship context, and situational framing between the agent and the user.",
            "agent:greeting" =>
                "Write or refine the opening message only. It should establish tone quickly and feel immersive.",
            "userProfile:description" =>
                "Describe the user profile in a compact, useful way that helps the agent respond appropriately.",
            "userProfile:traits" =>
                "List stable user traits and tendencies. Avoid temporary moods or repeating full description text.",
            "userProfile:preferences" =>
                "Describe interaction preferences, tone, pacing, or boundaries that improve response quality.",
            "userProfile:additionalinstructions" =>
                "Add extra guidance that does not belong in description, traits, or preferences.",
            _ =>
                "Improve the field for clarity, structure, and prompt usefulness without inventing unsupported facts.",
        };
    }

    private static string BuildRepairGuidance(string fieldName)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "agentdescription" => "Keep only stable overview information here.",
            "agentpersonalitydefinition" =>
                "Keep stable personality traits and behavioral tendencies here.",
            "agentscenario" => "Keep relationship and current situation framing here.",
            "agentgreeting" => "Keep only the immersive opening message here.",
            "userProfiledescription" => "Keep compact user-role overview here.",
            "userProfiletraits" => "Keep stable user traits here.",
            "userProfilepreferences" => "Keep tone/pacing/interaction preferences here.",
            "userProfileadditionalinstructions" =>
                "Keep extra guidance here without duplicating the other userProfile fields.",
            _ => "Repair the field so it fits its purpose cleanly.",
        };
    }

    private static (string SuggestedText, string? Rationale) ParseEnhancement(string raw)
    {
        var cleaned = ExtractJsonPayload(StripCodeFences(raw));

        try
        {
            using var doc = JsonDocument.Parse(cleaned);

            var suggestedText = doc.RootElement.TryGetProperty(
                "suggestedText",
                out var suggestedProp
            )
                ? suggestedProp.GetString()?.Trim()
                : null;

            var rationale = doc.RootElement.TryGetProperty("rationale", out var rationaleProp)
                ? rationaleProp.GetString()?.Trim()
                : null;

            if (!string.IsNullOrWhiteSpace(suggestedText))
            {
                return (suggestedText, rationale);
            }
        }
        catch { }

        return (raw.Trim(), "Model returned non-JSON output, so the raw response was used.");
    }

    private static FullAuthoringBundleGenerationResult ParseFullBundle(string raw)
    {
        var cleaned = ExtractJsonPayload(StripCodeFences(raw));

        try
        {
            using var doc = JsonDocument.Parse(cleaned);

            string Read(string key) =>
                doc.RootElement.TryGetProperty(key, out var prop)
                    ? prop.GetString()?.Trim() ?? string.Empty
                    : string.Empty;

            var result = new FullAuthoringBundleGenerationResult
            {
                AgentName = Read("agentName"),
                AgentDescription = Read("agentDescription"),
                AgentPersonalityDefinition = Read("agentPersonalityDefinition"),
                AgentScenario = Read("agentScenario"),
                AgentGreeting = Read("agentGreeting"),
                UserProfileDisplayName = Read("userProfileDisplayName"),
                UserProfileDescription = Read("userProfileDescription"),
                UserProfileTraits = Read("userProfileTraits"),
                UserProfilePreferences = Read("userProfilePreferences"),
                UserProfileAdditionalInstructions = Read("userProfileAdditionalInstructions"),
                Rationale = doc.RootElement.TryGetProperty("rationale", out var rationaleProp)
                    ? rationaleProp.GetString()?.Trim()
                    : null,
            };

            if (
                !string.IsNullOrWhiteSpace(result.AgentDescription)
                && !string.IsNullOrWhiteSpace(result.AgentPersonalityDefinition)
                && !string.IsNullOrWhiteSpace(result.AgentScenario)
                && !string.IsNullOrWhiteSpace(result.AgentGreeting)
            )
            {
                return result;
            }
        }
        catch { }

        return new FullAuthoringBundleGenerationResult
        {
            AgentName = string.Empty,
            AgentDescription = raw.Trim(),
            AgentPersonalityDefinition = string.Empty,
            AgentScenario = string.Empty,
            AgentGreeting = string.Empty,
            UserProfileDisplayName = string.Empty,
            UserProfileDescription = string.Empty,
            UserProfileTraits = string.Empty,
            UserProfilePreferences = string.Empty,
            UserProfileAdditionalInstructions = string.Empty,
            Rationale =
                "Model returned non-JSON output, so the raw response could not be fully structured.",
        };
    }

    private static AuthoringConsistencyCheckResult ParseConsistency(string raw, string entityType)
    {
        var cleaned = ExtractJsonPayload(StripCodeFences(raw));

        try
        {
            using var doc = JsonDocument.Parse(cleaned);

            var summary = doc.RootElement.TryGetProperty("summary", out var summaryProp)
                ? summaryProp.GetString()?.Trim()
                : "Consistency analysis completed.";

            var issues = new List<AuthoringConsistencyIssue>();

            if (
                doc.RootElement.TryGetProperty("issues", out var issuesProp)
                && issuesProp.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var issue in issuesProp.EnumerateArray())
                {
                    var severity = issue.TryGetProperty("severity", out var severityProp)
                        ? severityProp.GetString()?.Trim()
                        : "info";

                    var fieldName = issue.TryGetProperty("fieldName", out var fieldProp)
                        ? fieldProp.GetString()?.Trim()
                        : "unknown";

                    var issueType = issue.TryGetProperty("issueType", out var typeProp)
                        ? typeProp.GetString()?.Trim()
                        : "weak";

                    var description = issue.TryGetProperty("description", out var descriptionProp)
                        ? descriptionProp.GetString()?.Trim()
                        : "No description provided.";

                    var suggestion = issue.TryGetProperty("suggestion", out var suggestionProp)
                        ? suggestionProp.GetString()?.Trim()
                        : null;

                    issues.Add(
                        new AuthoringConsistencyIssue
                        {
                            Severity = string.IsNullOrWhiteSpace(severity) ? "info" : severity,
                            FieldName = string.IsNullOrWhiteSpace(fieldName)
                                ? "unknown"
                                : fieldName,
                            IssueType = string.IsNullOrWhiteSpace(issueType) ? "weak" : issueType,
                            Description = string.IsNullOrWhiteSpace(description)
                                ? "No description provided."
                                : description,
                            Suggestion = suggestion,
                        }
                    );
                }
            }

            return new AuthoringConsistencyCheckResult
            {
                EntityType = entityType,
                Summary = string.IsNullOrWhiteSpace(summary)
                    ? "Consistency analysis completed."
                    : summary,
                Issues = issues,
            };
        }
        catch
        {
            return new AuthoringConsistencyCheckResult
            {
                EntityType = entityType,
                Summary = "Consistency analysis returned non-JSON output.",
                Issues =
                [
                    new AuthoringConsistencyIssue
                    {
                        Severity = "warning",
                        FieldName = "general",
                        IssueType = "weak",
                        Description = raw.Trim(),
                        Suggestion = "Retry the consistency check.",
                    },
                ],
            };
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
}
