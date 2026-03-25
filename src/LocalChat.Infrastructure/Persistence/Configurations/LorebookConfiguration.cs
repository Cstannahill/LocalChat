using LocalChat.Domain.Entities.Lorebooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class LorebookConfiguration : IEntityTypeConfiguration<Lorebook>
{
    public void Configure(EntityTypeBuilder<Lorebook> builder)
    {
        builder.ToTable("Lorebooks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.CharacterId);

        builder
            .HasMany(x => x.Entries)
            .WithOne(x => x.Lorebook)
            .HasForeignKey(x => x.LorebookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
