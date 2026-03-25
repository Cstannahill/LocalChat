using LocalChat.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class CharacterSampleDialogueConfiguration
    : IEntityTypeConfiguration<CharacterSampleDialogue>
{
    public void Configure(EntityTypeBuilder<CharacterSampleDialogue> builder)
    {
        builder.ToTable("CharacterSampleDialogues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserMessage).IsRequired();

        builder.Property(x => x.AssistantMessage).IsRequired();

        builder.Property(x => x.SortOrder).IsRequired();

        builder.HasIndex(x => new { x.CharacterId, x.SortOrder }).IsUnique();
    }
}
