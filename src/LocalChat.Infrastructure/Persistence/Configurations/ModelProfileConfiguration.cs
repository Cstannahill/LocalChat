using LocalChat.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class ModelProfileConfiguration : IEntityTypeConfiguration<ModelProfile>
{
    public void Configure(EntityTypeBuilder<ModelProfile> builder)
    {
        builder.ToTable("ModelProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.Property(x => x.ProviderType).HasConversion<string>().IsRequired();

        builder.Property(x => x.ModelIdentifier).IsRequired().HasMaxLength(300);

        builder.Property(x => x.Notes).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Name);
    }
}
