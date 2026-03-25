using LocalChat.Domain.Entities.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class GeneratedImageAssetConfiguration : IEntityTypeConfiguration<GeneratedImageAsset>
{
    public void Configure(EntityTypeBuilder<GeneratedImageAsset> builder)
    {
        builder.ToTable("GeneratedImageAssets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RelativeUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ImageGenerationJobId);
    }
}
