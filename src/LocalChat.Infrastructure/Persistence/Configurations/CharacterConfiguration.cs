using LocalChat.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("Characters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Greeting)
            .IsRequired();

        builder.Property(x => x.PersonalityDefinition)
            .IsRequired();

        builder.Property(x => x.Scenario)
            .IsRequired();

        builder.Property(x => x.DefaultTtsVoice)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.DefaultVisualStylePreset)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.DefaultVisualPromptPrefix)
            .IsRequired(false);

        builder.Property(x => x.DefaultVisualNegativePrompt)
            .IsRequired(false);

        builder.Property(x => x.ImagePath)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.ImageUpdatedAt)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.DefaultModelProfileId);
        builder.HasIndex(x => x.DefaultGenerationPresetId);
        builder.HasIndex(x => x.ImagePath);

        builder.HasMany(x => x.SampleDialogues)
            .WithOne(x => x.Character)
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DefaultModelProfile)
            .WithMany()
            .HasForeignKey(x => x.DefaultModelProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.DefaultGenerationPreset)
            .WithMany()
            .HasForeignKey(x => x.DefaultGenerationPresetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
