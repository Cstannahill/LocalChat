using LocalChat.Domain.Entities.Audio;
using LocalChat.Domain.Entities.Agents;
using LocalChat.Domain.Entities.Conversations;
using LocalChat.Domain.Entities.Generation;
using LocalChat.Domain.Entities.Images;
using LocalChat.Domain.Entities.KnowledgeBases;
using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Entities.Models;
using LocalChat.Domain.Entities.UserProfiles;
using LocalChat.Domain.Entities.Retrieval;
using LocalChat.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace LocalChat.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<AgentSampleDialogue> AgentSampleDialogues => Set<AgentSampleDialogue>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<ModelProfile> ModelProfiles => Set<ModelProfile>();

    public DbSet<GenerationPreset> GenerationPresets => Set<GenerationPreset>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<MessageVariant> MessageVariants => Set<MessageVariant>();

    public DbSet<GenerationPromptSnapshot> GenerationPromptSnapshots => Set<GenerationPromptSnapshot>();

    public DbSet<SummaryCheckpoint> SummaryCheckpoints => Set<SummaryCheckpoint>();

    public DbSet<MemoryItem> MemoryItems => Set<MemoryItem>();

    public DbSet<MemoryOperationAudit> MemoryOperationAudits => Set<MemoryOperationAudit>();

    public DbSet<SessionStateExtractionEvent> SessionStateExtractionEvents => Set<SessionStateExtractionEvent>();

    public DbSet<MemoryExtractionAuditEvent> MemoryExtractionAuditEvents => Set<MemoryExtractionAuditEvent>();

    public DbSet<KnowledgeBase> KnowledgeBases => Set<KnowledgeBase>();

    public DbSet<LoreEntry> LoreEntries => Set<LoreEntry>();

    public DbSet<RetrievalChunk> RetrievalChunks => Set<RetrievalChunk>();

    public DbSet<SpeechClip> SpeechClips => Set<SpeechClip>();

    public DbSet<ImageGenerationJob> ImageGenerationJobs => Set<ImageGenerationJob>();

    public DbSet<GeneratedImageAsset> GeneratedImageAssets => Set<GeneratedImageAsset>();

    public DbSet<AppRuntimeDefaults> AppRuntimeDefaults => Set<AppRuntimeDefaults>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
