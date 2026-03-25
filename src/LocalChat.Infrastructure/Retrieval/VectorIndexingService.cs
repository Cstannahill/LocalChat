using LocalChat.Application.Abstractions.Inference;
using LocalChat.Application.Abstractions.Retrieval;
using LocalChat.Domain.Enums;
using LocalChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Retrieval;

public sealed class VectorIndexingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;

    public VectorIndexingService(
        ApplicationDbContext dbContext,
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore)
    {
        _dbContext = dbContext;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
    }

    public async Task<int> ReindexConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _dbContext.Conversations
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException($"Conversation '{conversationId}' was not found.");
        }

        await _vectorStore.DeleteByConversationSourceTypeAsync(
            conversationId,
            "ConversationMessage",
            cancellationToken);

        await _vectorStore.DeleteByConversationSourceTypeAsync(
            conversationId,
            "Memory",
            cancellationToken);

        var documents = new List<VectorDocument>();

        foreach (var message in conversation.Messages.OrderBy(x => x.SequenceNumber))
        {
            var content = $"{message.Role}: {message.Content}".Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var embedding = await _embeddingProvider.EmbedAsync(content, cancellationToken);

            documents.Add(new VectorDocument
            {
                SourceId = message.Id,
                SourceType = "ConversationMessage",
                AgentId = conversation.AgentId,
                ConversationId = conversation.Id,
                Content = content,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        var acceptedConversationMemories = await _dbContext.MemoryItems
            .Where(x =>
                x.ConversationId == conversationId &&
                x.ReviewStatus == MemoryReviewStatus.Accepted &&
                (x.Kind != MemoryKind.SessionState || x.SupersededAtSequenceNumber == null))
            .ToListAsync(cancellationToken);

        foreach (var memory in acceptedConversationMemories)
        {
            if (string.IsNullOrWhiteSpace(memory.Content))
            {
                continue;
            }

            var embedding = await _embeddingProvider.EmbedAsync(memory.Content, cancellationToken);

            documents.Add(new VectorDocument
            {
                SourceId = memory.Id,
                SourceType = "Memory",
                AgentId = memory.AgentId,
                ConversationId = memory.ConversationId,
                Content = memory.Content,
                Embedding = embedding,
                CreatedAt = memory.CreatedAt,
                UpdatedAt = memory.UpdatedAt
            });
        }

        await _vectorStore.UpsertAsync(documents, cancellationToken);
        return documents.Count;
    }

    public async Task<int> ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        await _vectorStore.DeleteAllAsync(cancellationToken);

        var documents = new List<VectorDocument>();

        var conversations = await _dbContext.Conversations
            .Include(x => x.Messages)
            .ToListAsync(cancellationToken);

        foreach (var conversation in conversations)
        {
            foreach (var message in conversation.Messages.OrderBy(x => x.SequenceNumber))
            {
                var content = $"{message.Role}: {message.Content}".Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                var embedding = await _embeddingProvider.EmbedAsync(content, cancellationToken);

                documents.Add(new VectorDocument
                {
                    SourceId = message.Id,
                    SourceType = "ConversationMessage",
                    AgentId = conversation.AgentId,
                    ConversationId = conversation.Id,
                    Content = content,
                    Embedding = embedding,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        var acceptedMemories = await _dbContext.MemoryItems
            .Where(x =>
                x.ReviewStatus == MemoryReviewStatus.Accepted &&
                (x.Kind != MemoryKind.SessionState || x.SupersededAtSequenceNumber == null))
            .ToListAsync(cancellationToken);

        foreach (var memory in acceptedMemories)
        {
            if (string.IsNullOrWhiteSpace(memory.Content))
            {
                continue;
            }

            var embedding = await _embeddingProvider.EmbedAsync(memory.Content, cancellationToken);

            documents.Add(new VectorDocument
            {
                SourceId = memory.Id,
                SourceType = "Memory",
                AgentId = memory.AgentId,
                ConversationId = memory.ConversationId,
                Content = memory.Content,
                Embedding = embedding,
                CreatedAt = memory.CreatedAt,
                UpdatedAt = memory.UpdatedAt
            });
        }

        var enabledLoreEntries = await _dbContext.LoreEntries
            .Where(x => x.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var lore in enabledLoreEntries)
        {
            var content = $"{lore.Title}: {lore.Content}".Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var embedding = await _embeddingProvider.EmbedAsync(content, cancellationToken);

            documents.Add(new VectorDocument
            {
                SourceId = lore.Id,
                SourceType = "Lore",
                AgentId = null,
                ConversationId = null,
                Content = content,
                Embedding = embedding,
                CreatedAt = lore.CreatedAt,
                UpdatedAt = lore.UpdatedAt
            });
        }

        await _vectorStore.UpsertAsync(documents, cancellationToken);
        return documents.Count;
    }
}
