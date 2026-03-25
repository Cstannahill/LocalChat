using LocalChat.Domain.Entities.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class MemoryOperationAuditConfiguration : IEntityTypeConfiguration<MemoryOperationAudit>
{
    public void Configure(EntityTypeBuilder<MemoryOperationAudit> builder)
    {
        builder.ToTable("MemoryOperationAudits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MemoryItemId)
            .IsRequired();

        builder.Property(x => x.SourceMemoryItemId)
            .IsRequired(false);

        builder.Property(x => x.TargetMemoryItemId)
            .IsRequired(false);

        builder.Property(x => x.OperationType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ConversationId)
            .IsRequired(false);

        builder.Property(x => x.CharacterId)
            .IsRequired(false);

        builder.Property(x => x.MessageSequenceNumber)
            .IsRequired(false);

        builder.Property(x => x.BeforeStateJson)
            .IsRequired(false);

        builder.Property(x => x.AfterStateJson)
            .IsRequired(false);

        builder.Property(x => x.Note)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.IsUndone)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.UndoneAtUtc)
            .IsRequired(false);

        builder.Property(x => x.UndoAuditId)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.MemoryItemId);
        builder.HasIndex(x => x.SourceMemoryItemId);
        builder.HasIndex(x => x.TargetMemoryItemId);
        builder.HasIndex(x => x.OperationType);
        builder.HasIndex(x => x.CreatedAt);
    }
}
