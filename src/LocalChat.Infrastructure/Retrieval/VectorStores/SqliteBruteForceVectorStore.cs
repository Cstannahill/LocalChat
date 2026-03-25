using System.Text.Json;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Retrieval.VectorStores;

public sealed class SqliteBruteForceVectorStore : IVectorStore
{
    private static readonly Guid GlobalCharacterId = Guid.Empty;

    private readonly ApplicationDbContext _dbContext;

    public SqliteBruteForceVectorStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertAsync(
        IReadOnlyList<VectorDocument> documents,
        CancellationToken cancellationToken = default
    )
    {
        if (documents.Count == 0)
        {
            return;
        }

        foreach (var document in documents)
        {
            var existing = await _dbContext.RetrievalChunks
                .Where(x => x.SourceType == document.SourceType && x.SourceEntityId == document.SourceId)
                .ToListAsync(cancellationToken);

            if (existing.Count > 0)
            {
                _dbContext.RetrievalChunks.RemoveRange(existing);
            }

            _dbContext.RetrievalChunks.Add(new Domain.Entities.Retrieval.RetrievalChunk
            {
                Id = Guid.NewGuid(),
                SourceType = document.SourceType,
                SourceEntityId = document.SourceId,
                CharacterId = document.CharacterId ?? GlobalCharacterId,
                ConversationId = document.ConversationId,
                Content = document.Content,
                EmbeddingJson = JsonSerializer.Serialize(document.Embedding),
                IsEnabled = true,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await _dbContext.RetrievalChunks
            .Where(x => x.SourceType == sourceType && x.SourceEntityId == sourceId)
            .ToListAsync(cancellationToken);

        if (existing.Count == 0)
        {
            return;
        }

        _dbContext.RetrievalChunks.RemoveRange(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await _dbContext.RetrievalChunks
            .Where(x => x.ConversationId == conversationId)
            .ToListAsync(cancellationToken);

        if (existing.Count == 0)
        {
            return;
        }

        _dbContext.RetrievalChunks.RemoveRange(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByConversationSourceTypeAsync(
        Guid conversationId,
        string sourceType,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await _dbContext.RetrievalChunks
            .Where(x => x.ConversationId == conversationId && x.SourceType == sourceType)
            .ToListAsync(cancellationToken);

        if (existing.Count == 0)
        {
            return;
        }

        _dbContext.RetrievalChunks.RemoveRange(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.RetrievalChunks.ToListAsync(cancellationToken);
        if (existing.Count == 0)
        {
            return;
        }

        _dbContext.RetrievalChunks.RemoveRange(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var retrievalQuery = _dbContext.RetrievalChunks
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (query.SourceTypes.Count > 0)
        {
            retrievalQuery = retrievalQuery.Where(x => query.SourceTypes.Contains(x.SourceType));
        }

        if (query.CharacterId.HasValue)
        {
            retrievalQuery = query.IncludeGlobalCharacterItems
                ? retrievalQuery.Where(x =>
                    x.CharacterId == query.CharacterId.Value || x.CharacterId == GlobalCharacterId
                )
                : retrievalQuery.Where(x => x.CharacterId == query.CharacterId.Value);
        }

        if (query.ConversationId.HasValue)
        {
            retrievalQuery = query.IncludeGlobalConversationItems
                ? retrievalQuery.Where(x =>
                    x.ConversationId == null || x.ConversationId == query.ConversationId
                )
                : retrievalQuery.Where(x => x.ConversationId == query.ConversationId);
        }
        else if (!query.IncludeGlobalConversationItems)
        {
            retrievalQuery = retrievalQuery.Where(x => x.ConversationId == null);
        }

        var chunks = await retrievalQuery.ToListAsync(cancellationToken);

        return chunks
            .Select(x => new
            {
                Chunk = x,
                Embedding = DeserializeVector(x.EmbeddingJson)
            })
            .Where(x => x.Embedding is not null && x.Embedding.Length == query.QueryEmbedding.Length)
            .Select(x => new VectorSearchResult
            {
                SourceId = x.Chunk.SourceEntityId,
                SourceType = x.Chunk.SourceType,
                CharacterId = x.Chunk.CharacterId == GlobalCharacterId ? null : x.Chunk.CharacterId,
                ConversationId = x.Chunk.ConversationId,
                Content = x.Chunk.Content,
                UpdatedAt = x.Chunk.UpdatedAt,
                SemanticScore = CosineSimilarityNormalized(query.QueryEmbedding, x.Embedding!)
            })
            .OrderByDescending(x => x.SemanticScore)
            .Take(Math.Max(1, query.TopK))
            .ToList();
    }

    private static float[]? DeserializeVector(string json)
    {
        try
        {
            var vector = JsonSerializer.Deserialize<float[]>(json);
            if (vector is { Length: > 0 })
            {
                return vector;
            }
        }
        catch
        {
        }

        try
        {
            var vector = JsonSerializer.Deserialize<double[]>(json);
            if (vector is { Length: > 0 })
            {
                return vector.Select(x => (float)x).ToArray();
            }
        }
        catch
        {
        }

        return null;
    }

    private static double CosineSimilarityNormalized(float[] query, float[] candidate)
    {
        double dot = 0;
        double queryNorm = 0;
        double candidateNorm = 0;

        for (var i = 0; i < query.Length; i++)
        {
            dot += query[i] * candidate[i];
            queryNorm += query[i] * query[i];
            candidateNorm += candidate[i] * candidate[i];
        }

        if (queryNorm <= 0 || candidateNorm <= 0)
        {
            return 0.0;
        }

        var cosine = dot / (Math.Sqrt(queryNorm) * Math.Sqrt(candidateNorm));
        return Math.Max(0.0, (cosine + 1.0) / 2.0);
    }
}
