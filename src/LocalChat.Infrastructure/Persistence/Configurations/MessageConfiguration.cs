using LocalChat.Domain.Entities.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<string>().IsRequired();

        builder.Property(x => x.OriginType).HasConversion<string>().IsRequired();

        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.SequenceNumber).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.SelectedVariantIndex).IsRequired(false);

        builder.HasIndex(x => new { x.ConversationId, x.SequenceNumber }).IsUnique();

        builder
            .HasMany(x => x.Variants)
            .WithOne(x => x.Message)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
