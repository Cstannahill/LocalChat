using LocalChat.Domain.Entities.Memory;
using LocalChat.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class MemoryItemConfiguration : IEntityTypeConfiguration<MemoryItem>
{
    public void Configure(EntityTypeBuilder<MemoryItem> builder)
    {
        builder.ToTable("MemoryItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasDefaultValue(MemoryKind.DurableFact)
            .IsRequired();

        builder.Property(x => x.ScopeType)
            .HasConversion<string>()
            .HasDefaultValue(MemoryScopeType.Conversation)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.ReviewStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ProposalReason)
            .IsRequired(false);

        builder.Property(x => x.SourceExcerpt)
            .IsRequired(false);

        builder.Property(x => x.NormalizedKey)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.SlotKey)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.SlotFamily)
            .HasConversion<string>()
            .HasDefaultValue(MemorySlotFamily.None)
            .IsRequired();

        builder.Property(x => x.ConfidenceScore)
            .IsRequired(false);

        builder.Property(x => x.SourceMessageSequenceNumber)
            .IsRequired(false);

        builder.Property(x => x.LastObservedSequenceNumber)
            .IsRequired(false);

        builder.Property(x => x.SupersededAtSequenceNumber)
            .IsRequired(false);

        builder.Property(x => x.ExpiresAt)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.ReviewStatus);
        builder.HasIndex(x => x.Kind);
        builder.HasIndex(x => x.ScopeType);
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => x.SlotKey);
        builder.HasIndex(x => x.SlotFamily);
        builder.HasIndex(x => x.SourceMessageSequenceNumber);
        builder.HasIndex(x => x.LastObservedSequenceNumber);
        builder.HasIndex(x => x.SupersededAtSequenceNumber);
        builder.HasIndex(x => new { x.CharacterId, x.ConversationId, x.Kind, x.NormalizedKey });
        builder.HasIndex(x => new { x.CharacterId, x.ConversationId, x.Kind, x.SlotKey });
        builder.HasIndex(x => new { x.CharacterId, x.ConversationId, x.Kind, x.SlotFamily });

        builder.HasOne<MemoryItem>()
            .WithMany()
            .HasForeignKey(x => x.ConflictsWithMemoryItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
