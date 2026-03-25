using System.Diagnostics;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Retrieval.Admin;

public sealed class RetrievalAdminService : IRetrievalAdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly VectorIndexingService _vectorIndexingService;

    public RetrievalAdminService(
        ApplicationDbContext dbContext,
        VectorIndexingService vectorIndexingService
    )
    {
        _dbContext = dbContext;
        _vectorIndexingService = vectorIndexingService;
    }

    public async Task<RetrievalAdminStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        return new RetrievalAdminStats
        {
            ConversationCount = await _dbContext.Conversations.CountAsync(cancellationToken),
            MessageCount = await _dbContext.Messages.CountAsync(cancellationToken),
            AcceptedMemoryCount = await _dbContext.MemoryItems
                .CountAsync(x => x.ReviewStatus == MemoryReviewStatus.Accepted, cancellationToken),
            ProposedMemoryCount = await _dbContext.MemoryItems
                .CountAsync(x => x.ReviewStatus == MemoryReviewStatus.Proposed, cancellationToken),
            EnabledLoreEntryCount = await _dbContext.LoreEntries
                .CountAsync(x => x.IsEnabled, cancellationToken),
            RetrievalChunkCount = await _dbContext.RetrievalChunks.CountAsync(cancellationToken)
        };
    }

    public async Task<RetrievalReindexResult> ReindexConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var rebuilt = await _vectorIndexingService.ReindexConversationAsync(
            conversationId,
            cancellationToken
        );
        stopwatch.Stop();

        return new RetrievalReindexResult
        {
            Scope = "conversation",
            ConversationId = conversationId,
            UpdatedChunkCount = rebuilt,
            SkippedChunkCount = 0,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    public async Task<RetrievalReindexResult> ReindexAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var rebuilt = await _vectorIndexingService.ReindexAllAsync(cancellationToken);
        stopwatch.Stop();

        return new RetrievalReindexResult
        {
            Scope = "all",
            ConversationId = null,
            UpdatedChunkCount = rebuilt,
            SkippedChunkCount = 0,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }
}
