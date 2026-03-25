using LocalChat.Domain.Entities.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class SummaryCheckpointConfiguration : IEntityTypeConfiguration<SummaryCheckpoint>
{
    public void Configure(EntityTypeBuilder<SummaryCheckpoint> builder)
    {
        builder.ToTable("SummaryCheckpoints");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartSequenceNumber)
            .IsRequired();

        builder.Property(x => x.EndSequenceNumber)
            .IsRequired();

        builder.Property(x => x.SummaryText)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.ConversationId, x.EndSequenceNumber });

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.SummaryCheckpoints)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}