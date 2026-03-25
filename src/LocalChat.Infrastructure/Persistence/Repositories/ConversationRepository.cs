using LocalChat.Application.Abstractions.Persistence;
using LocalChat.Domain.Entities.Conversations;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence.Repositories;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ConversationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation?> GetByIdWithMessagesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .Include(x => x.Agent)
                .ThenInclude(x => x!.SampleDialogues)
            .Include(x => x.UserProfile)
            .Include(x => x.Messages.OrderBy(m => m.SequenceNumber))
                .ThenInclude(x => x.Variants.OrderBy(v => v.VariantIndex))
            .Include(x => x.SummaryCheckpoints.OrderBy(s => s.EndSequenceNumber))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Conversation?> GetByMessageIdWithMessagesAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var conversationId = await _dbContext.Messages
            .Where(x => x.Id == messageId)
            .Select(x => (Guid?)x.ConversationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!conversationId.HasValue)
        {
            return null;
        }

        return await GetByIdWithMessagesAsync(conversationId.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> ListByAgentAsync(
        Guid agentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.AgentId == agentId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SummaryCheckpoint?> GetLatestSummaryAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SummaryCheckpoints
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.EndSequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Conversation> AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Conversations.AddAsync(conversation, cancellationToken);
        return conversation;
    }

    public async Task AddMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Messages.AddAsync(message, cancellationToken);
    }

    public async Task AddMessageVariantAsync(
        MessageVariant variant,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.MessageVariants.AddAsync(variant, cancellationToken);
    }

    public async Task AddSummaryCheckpointAsync(
        SummaryCheckpoint checkpoint,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SummaryCheckpoints.AddAsync(checkpoint, cancellationToken);
    }

    public async Task<int> DeleteMessagesFromSequenceAsync(
        Guid conversationId,
        int sequenceNumber,
        bool inclusive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Messages
            .Where(x => x.ConversationId == conversationId);

        query = inclusive
            ? query.Where(x => x.SequenceNumber >= sequenceNumber)
            : query.Where(x => x.SequenceNumber > sequenceNumber);

        var messages = await query.ToListAsync(cancellationToken);
        if (messages.Count == 0)
        {
            return 0;
        }

        _dbContext.Messages.RemoveRange(messages);
        return messages.Count;
    }

    public async Task DeleteMessageVariantsAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var variants = await _dbContext.MessageVariants
            .Where(x => x.MessageId == messageId)
            .ToListAsync(cancellationToken);

        if (variants.Count == 0)
        {
            return;
        }

        _dbContext.MessageVariants.RemoveRange(variants);
    }

    public async Task<int> DeleteSummaryCheckpointsAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var checkpoints = await _dbContext.SummaryCheckpoints
            .Where(x => x.ConversationId == conversationId)
            .ToListAsync(cancellationToken);

        if (checkpoints.Count == 0)
        {
            return 0;
        }

        _dbContext.SummaryCheckpoints.RemoveRange(checkpoints);
        return checkpoints.Count;
    }

    public async Task<int> GetNextSequenceNumberAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var maxSequence = await _dbContext.Messages
            .Where(x => x.ConversationId == conversationId)
            .Select(x => (int?)x.SequenceNumber)
            .MaxAsync(cancellationToken);

        return (maxSequence ?? 0) + 1;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
