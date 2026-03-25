using LocalChat.Domain.Entities.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class MessageVariantConfiguration : IEntityTypeConfiguration<MessageVariant>
{
    public void Configure(EntityTypeBuilder<MessageVariant> builder)
    {
        builder.ToTable("MessageVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.VariantIndex).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.ModelIdentifier)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.ModelProfileId)
            .IsRequired(false);

        builder.Property(x => x.GenerationPresetId)
            .IsRequired(false);

        builder.Property(x => x.ProviderType)
            .IsRequired(false);

        builder.Property(x => x.RuntimeSourceType)
            .IsRequired(false);

        builder.Property(x => x.GenerationStartedAt)
            .IsRequired(false);

        builder.Property(x => x.GenerationCompletedAt)
            .IsRequired(false);

        builder.Property(x => x.ResponseTimeMs)
            .IsRequired(false);

        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => new { x.MessageId, x.VariantIndex }).IsUnique();
        builder.HasIndex(x => x.ProviderType);
        builder.HasIndex(x => x.ModelIdentifier);
    }
}
