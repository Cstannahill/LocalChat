using System.Text;
using LocalChat.Infrastructure.Options;

namespace LocalChat.Infrastructure.Retrieval.Ranking;

public sealed class RetrievalRanker
{
    private readonly RetrievalOptions _options;

    public RetrievalRanker(RetrievalOptions options)
    {
        _options = options;
    }

    public RetrievalScoreBreakdown ScoreMemory(
        string query,
        string content,
        DateTime updatedAt,
        bool isPinned,
        double semanticScore
    )
    {
        var lexical = ComputeLexicalScore(query, content);
        var recency = ComputeRecencyScore(updatedAt);

        var sourceBoost = _options.MemoryBaseBoost + (isPinned ? _options.PinnedMemoryBoost : 0.0);
        var finalScore =
            (semanticScore * _options.SemanticWeight)
            + (lexical * _options.LexicalWeight)
            + (recency * _options.RecencyWeight)
            + sourceBoost;

        return new RetrievalScoreBreakdown
        {
            SemanticScore = semanticScore,
            LexicalScore = lexical,
            RecencyScore = recency,
            SourceBoost = sourceBoost,
            FinalScore = finalScore,
            Reason = BuildReason(
                semanticScore,
                lexical,
                recency,
                isPinned ? "Pinned memory" : "Memory"
            ),
        };
    }

    public RetrievalScoreBreakdown ScoreLore(
        string query,
        string content,
        DateTime updatedAt,
        double semanticScore
    )
    {
        var lexical = ComputeLexicalScore(query, content);
        var recency = ComputeRecencyScore(updatedAt);

        var sourceBoost = _options.LoreBaseBoost;
        var finalScore =
            (semanticScore * _options.SemanticWeight)
            + (lexical * _options.LexicalWeight)
            + (recency * _options.RecencyWeight)
            + sourceBoost;

        return new RetrievalScoreBreakdown
        {
            SemanticScore = semanticScore,
            LexicalScore = lexical,
            RecencyScore = recency,
            SourceBoost = sourceBoost,
            FinalScore = finalScore,
            Reason = BuildReason(semanticScore, lexical, recency, "Lore"),
        };
    }

    private double ComputeLexicalScore(string query, string content)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(content))
        {
            return 0.0;
        }

        var queryTokens = Tokenize(query);
        var contentTokens = Tokenize(content);

        if (queryTokens.Count == 0 || contentTokens.Count == 0)
        {
            return 0.0;
        }

        var overlapCount = queryTokens.Intersect(contentTokens).Count();
        var overlapRatio = (double)overlapCount / queryTokens.Count;

        var normalizedQuery = NormalizeInline(query);
        var normalizedContent = NormalizeInline(content);

        var phraseBonus =
            normalizedQuery.Length >= 8
            && normalizedContent.Contains(normalizedQuery, StringComparison.Ordinal)
                ? _options.QueryPhraseBoost
                : 0.0;

        var exactTokenBonus = Math.Min(1.0, overlapCount / 3.0) * _options.ExactTokenBoost;

        return Math.Min(1.25, overlapRatio + phraseBonus + exactTokenBonus);
    }

    private double ComputeRecencyScore(DateTime updatedAt)
    {
        var ageDays = Math.Max(0.0, (DateTime.UtcNow - updatedAt).TotalDays);
        var halfLife = Math.Max(1.0, _options.RecencyHalfLifeDays);

        return Math.Pow(0.5, ageDays / halfLife);
    }

    private HashSet<string> Tokenize(string input)
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        var sb = new StringBuilder();

        foreach (var ch in input.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
            else if (sb.Length > 0)
            {
                AddToken(sb, tokens);
            }
        }

        if (sb.Length > 0)
        {
            AddToken(sb, tokens);
        }

        return tokens;
    }

    private void AddToken(StringBuilder sb, HashSet<string> tokens)
    {
        var token = sb.ToString();
        sb.Clear();

        if (token.Length >= _options.MinTokenLength)
        {
            tokens.Add(token);
        }
    }

    private static string NormalizeInline(string input)
    {
        var sb = new StringBuilder(input.Length);

        foreach (var ch in input.ToLowerInvariant())
        {
            sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
        }

        return string.Join(
            ' ',
            sb.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        );
    }

    private static string BuildReason(double semantic, double lexical, double recency, string label)
    {
        var parts = new List<string> { label };

        if (semantic >= 0.60)
        {
            parts.Add("strong semantic match");
        }
        else if (semantic >= 0.35)
        {
            parts.Add("moderate semantic match");
        }

        if (lexical >= 0.80)
        {
            parts.Add("strong lexical overlap");
        }
        else if (lexical >= 0.40)
        {
            parts.Add("some lexical overlap");
        }

        if (recency >= 0.70)
        {
            parts.Add("recent");
        }

        return string.Join(" • ", parts);
    }
}

public sealed class RetrievalScoreBreakdown
{
    public required double SemanticScore { get; init; }

    public required double LexicalScore { get; init; }

    public required double RecencyScore { get; init; }

    public required double SourceBoost { get; init; }

    public required double FinalScore { get; init; }

    public required string Reason { get; init; }
}
