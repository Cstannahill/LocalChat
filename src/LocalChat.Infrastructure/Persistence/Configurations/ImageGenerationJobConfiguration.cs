using LocalChat.Domain.Entities.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class ImageGenerationJobConfiguration : IEntityTypeConfiguration<ImageGenerationJob>
{
    public void Configure(EntityTypeBuilder<ImageGenerationJob> builder)
    {
        builder.ToTable("ImageGenerationJobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PromptText)
            .IsRequired();

        builder.Property(x => x.NegativePromptText)
            .IsRequired();

        builder.Property(x => x.ProviderJobId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Character)
            .WithMany()
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Assets)
            .WithOne(x => x.ImageGenerationJob)
            .HasForeignKey(x => x.ImageGenerationJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
