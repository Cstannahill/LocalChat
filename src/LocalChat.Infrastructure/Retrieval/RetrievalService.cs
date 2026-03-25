using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Application.Inspection;
using LocalChat.Application.Memory;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Options;
using LocalChat.Infrastructure.Persistence;
using LocalChat.Infrastructure.Retrieval.Ranking;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Retrieval;

public sealed class RetrievalService : IRetrievalService
{
    private const string MemorySourceType = "Memory";
    private const string LoreSourceType = "Lore";

    private readonly ApplicationDbContext _dbContext;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly RetrievalRanker _ranker;
    private readonly RetrievalOptions _options;
    private readonly IMemoryPolicyService _memoryPolicyService;

    public RetrievalService(
        ApplicationDbContext dbContext,
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore,
        RetrievalRanker ranker,
        RetrievalOptions options,
        IMemoryPolicyService memoryPolicyService
    )
    {
        _dbContext = dbContext;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _ranker = ranker;
        _options = options;
        _memoryPolicyService = memoryPolicyService;
    }

    public async Task IndexMemoryAsync(
        MemoryItem memoryItem,
        CancellationToken cancellationToken = default
    )
    {
        if (
            memoryItem.ReviewStatus != MemoryReviewStatus.Accepted
            || !memoryItem.AgentId.HasValue
            || string.IsNullOrWhiteSpace(memoryItem.Content)
        )
        {
            await _vectorStore.DeleteBySourceAsync(MemorySourceType, memoryItem.Id, cancellationToken);
            return;
        }

        var embedding = await _embeddingProvider.EmbedAsync(memoryItem.Content, cancellationToken);

        await _vectorStore.UpsertAsync(
            new[]
            {
                new VectorDocument
                {
                    SourceId = memoryItem.Id,
                    SourceType = MemorySourceType,
                    AgentId = memoryItem.AgentId.Value,
                    ConversationId = memoryItem.ConversationId,
                    Content = memoryItem.Content,
                    Embedding = embedding,
                    CreatedAt = memoryItem.CreatedAt,
                    UpdatedAt = memoryItem.UpdatedAt
                }
            },
            cancellationToken
        );
    }

    public async Task IndexLoreEntryAsync(
        Guid agentId,
        LoreEntry loreEntry,
        CancellationToken cancellationToken = default
    )
    {
        if (!loreEntry.IsEnabled)
        {
            await _vectorStore.DeleteBySourceAsync(LoreSourceType, loreEntry.Id, cancellationToken);
            return;
        }

        var content = $"{loreEntry.Title}: {loreEntry.Content}".Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            await _vectorStore.DeleteBySourceAsync(LoreSourceType, loreEntry.Id, cancellationToken);
            return;
        }

        var embedding = await _embeddingProvider.EmbedAsync(content, cancellationToken);

        await _vectorStore.UpsertAsync(
            new[]
            {
                new VectorDocument
                {
                    SourceId = loreEntry.Id,
                    SourceType = LoreSourceType,
                    AgentId = agentId,
                    ConversationId = null,
                    Content = content,
                    Embedding = embedding,
                    CreatedAt = loreEntry.CreatedAt,
                    UpdatedAt = loreEntry.UpdatedAt
                }
            },
            cancellationToken
        );
    }

    public Task RemoveSourceAsync(
        string sourceType,
        Guid sourceEntityId,
        CancellationToken cancellationToken = default
    )
    {
        return _vectorStore.DeleteBySourceAsync(sourceType, sourceEntityId, cancellationToken);
    }

    public async Task<RetrievalInspectionResult> InspectAsync(
        Guid agentId,
        Guid? conversationId,
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(query))
        {
            return new RetrievalInspectionResult
            {
                Query = query,
                SelectedMemories = Array.Empty<MemoryItem>(),
                SelectedLoreEntries = Array.Empty<LoreEntry>(),
                SelectedMemoryExplanations = Array.Empty<SelectedMemoryExplanation>(),
                SelectedLoreExplanations = Array.Empty<SelectedLoreExplanation>()
            };
        }

        var queryEmbedding = await _embeddingProvider.EmbedAsync(query, cancellationToken);

        var memoryCandidates = await _vectorStore.SearchAsync(
            new VectorSearchQuery
            {
                QueryEmbedding = queryEmbedding,
                SourceTypes = new[] { MemorySourceType },
                AgentId = agentId,
                ConversationId = conversationId,
                IncludeGlobalAgentItems = true,
                IncludeGlobalConversationItems = true,
                TopK = _options.CandidatePoolSize
            },
            cancellationToken
        );

        var loreCandidates = await _vectorStore.SearchAsync(
            new VectorSearchQuery
            {
                QueryEmbedding = queryEmbedding,
                SourceTypes = new[] { LoreSourceType },
                AgentId = agentId,
                ConversationId = null,
                IncludeGlobalAgentItems = true,
                IncludeGlobalConversationItems = true,
                TopK = _options.CandidatePoolSize
            },
            cancellationToken
        );

        var memoryIds = memoryCandidates.Select(x => x.SourceId).Distinct().ToList();
        var loreIds = loreCandidates.Select(x => x.SourceId).Distinct().ToList();

        var acceptedMemories = await _dbContext.MemoryItems
            .AsNoTracking()
            .Where(x =>
                memoryIds.Contains(x.Id) &&
                x.ReviewStatus == MemoryReviewStatus.Accepted)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var currentSequenceNumber = 0;
        if (conversationId.HasValue)
        {
            currentSequenceNumber = await _dbContext.Messages
                .AsNoTracking()
                .Where(x => x.ConversationId == conversationId.Value)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken) ?? 0;
        }

        var enabledLoreEntries = await _dbContext.LoreEntries
            .AsNoTracking()
            .Where(x => loreIds.Contains(x.Id) && x.IsEnabled)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var memorySemanticScores = memoryCandidates
            .GroupBy(x => x.SourceId)
            .ToDictionary(
                g => g.Key,
                g => g.Max(x => x.SemanticScore));

        var memoryRanking = MemoryCandidateRanker.Rank(
            acceptedMemories.Values.ToList(),
            memorySemanticScores,
            conversationId,
            currentSequenceNumber,
            _memoryPolicyService,
            maxCount: 8);

        var selectedMemories = memoryRanking.Selected
            .Select(x => x.Memory)
            .ToList();

        var selectedMemoryExplanations = memoryRanking.Selected
            .Select(x => new SelectedMemoryExplanation
            {
                MemoryId = x.Memory.Id,
                Category = x.Memory.Category.ToString(),
                Kind = x.Memory.Kind.ToString(),
                Content = x.Memory.Content,
                SlotKey = x.Memory.SlotKey,
                SemanticScore = x.SemanticScore,
                FinalScore = x.FinalScore,
                WhySelected = x.WhySelected,
                SuppressedMemories = x.SuppressedMemories
                    .Select(s => new SuppressedMemoryDetail
                    {
                        MemoryId = s.Memory.Id,
                        Category = s.Memory.Category.ToString(),
                        Kind = s.Memory.Kind.ToString(),
                        Content = s.Memory.Content,
                        SlotKey = s.Memory.SlotKey,
                        FinalScore = s.FinalScore,
                        Reason = s.Reason
                    })
                    .ToList()
            })
            .ToList();

        var rankedLore = loreCandidates
            .Select(candidate =>
            {
                if (!enabledLoreEntries.TryGetValue(candidate.SourceId, out var lore))
                {
                    return null;
                }

                var score = _ranker.ScoreLore(
                    query,
                    candidate.Content,
                    candidate.UpdatedAt,
                    candidate.SemanticScore
                );

                return new ScoredLoreCandidate
                {
                    Lore = lore,
                    PreviewText = BuildPreview(candidate.Content),
                    Score = score
                };
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.Score.FinalScore)
            .ToList();

        var selectedLore = rankedLore
            .Where(x =>
                x.Score.FinalScore >= _options.MinFinalScore
                && (
                    x.Score.SemanticScore >= _options.MinSemanticScore
                    || x.Score.LexicalScore >= _options.StrongLexicalScore
                )
            )
            .Take(_options.MaxSelectedLoreEntries)
            .ToList();

        if (
            selectedLore.Count == 0
            && rankedLore.Count > 0
            && rankedLore[0].Score.FinalScore >= _options.FallbackMinFinalScore
        )
        {
            selectedLore.Add(rankedLore[0]);
        }

        var selectedLoreEntries = selectedLore.Select(x => x.Lore).ToList();

        var loreSemanticScores = loreCandidates
            .GroupBy(x => x.SourceId)
            .ToDictionary(
                g => g.Key,
                g => g.Max(x => x.SemanticScore));

        var selectedLoreExplanations = selectedLoreEntries
            .Select(x =>
            {
                var semanticScore = loreSemanticScores.TryGetValue(x.Id, out var value) ? value : 0.0;
                var finalScore = semanticScore + 0.03;

                return new SelectedLoreExplanation
                {
                    LoreEntryId = x.Id,
                    Title = x.Title,
                    Content = x.Content,
                    SemanticScore = semanticScore,
                    FinalScore = finalScore,
                    WhySelected =
                        $"Selected because the lore entry semantically matched the query/context. " +
                        $"Semantic score: {semanticScore:0.000}. Final score: {finalScore:0.000}."
                };
            })
            .ToList();

        return new RetrievalInspectionResult
        {
            Query = query,
            SelectedMemories = selectedMemories,
            SelectedLoreEntries = selectedLoreEntries,
            SelectedMemoryExplanations = selectedMemoryExplanations,
            SelectedLoreExplanations = selectedLoreExplanations
        };
    }

    private static string BuildPreview(string content)
    {
        var trimmed = content.Trim().Replace("\r", " ").Replace("\n", " ");
        return trimmed.Length <= 180 ? trimmed : $"{trimmed[..180]}...";
    }

    private sealed class ScoredLoreCandidate
    {
        public required LoreEntry Lore { get; init; }

        public required string PreviewText { get; init; }

        public required RetrievalScoreBreakdown Score { get; init; }
    }
}
