using LocalChat.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class GenerationPresetConfiguration : IEntityTypeConfiguration<GenerationPreset>
{
    public void Configure(EntityTypeBuilder<GenerationPreset> builder)
    {
        builder.ToTable("GenerationPresets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.Property(x => x.Temperature).IsRequired();

        builder.Property(x => x.TopP).IsRequired();

        builder.Property(x => x.RepeatPenalty).IsRequired();

        builder.Property(x => x.StopSequencesText).IsRequired();

        builder.Property(x => x.Notes).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Name);
    }
}
