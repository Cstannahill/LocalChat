using LocalChat.Domain.Entities.Generation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class GenerationPromptSnapshotConfiguration : IEntityTypeConfiguration<GenerationPromptSnapshot>
{
    public void Configure(EntityTypeBuilder<GenerationPromptSnapshot> builder)
    {
        builder.ToTable("GenerationPromptSnapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageVariantId)
            .IsRequired();

        builder.Property(x => x.MessageId)
            .IsRequired();

        builder.Property(x => x.ConversationId)
            .IsRequired();

        builder.Property(x => x.FullPromptText)
            .IsRequired();

        builder.Property(x => x.PromptSectionsJson)
            .IsRequired();

        builder.Property(x => x.EstimatedPromptTokens)
            .IsRequired();

        builder.Property(x => x.ResolvedContextWindow)
            .IsRequired(false);

        builder.Property(x => x.ProviderType)
            .IsRequired(false);

        builder.Property(x => x.ModelIdentifier)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.ModelProfileId)
            .IsRequired(false);

        builder.Property(x => x.GenerationPresetId)
            .IsRequired(false);

        builder.Property(x => x.RuntimeSourceType)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.MessageVariantId).IsUnique();
        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ProviderType);
        builder.HasIndex(x => x.ModelIdentifier);
    }
}
