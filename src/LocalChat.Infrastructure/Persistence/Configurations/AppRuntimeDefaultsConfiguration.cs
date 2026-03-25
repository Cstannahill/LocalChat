using LocalChat.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class AppRuntimeDefaultsConfiguration : IEntityTypeConfiguration<AppRuntimeDefaults>
{
    public void Configure(EntityTypeBuilder<AppRuntimeDefaults> builder)
    {
        builder.ToTable("AppRuntimeDefaults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DefaultPersonaId)
            .IsRequired(false);

        builder.Property(x => x.DefaultModelProfileId)
            .IsRequired(false);

        builder.Property(x => x.DefaultGenerationPresetId)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
