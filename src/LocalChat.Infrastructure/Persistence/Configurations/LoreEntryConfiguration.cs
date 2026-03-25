using LocalChat.Domain.Entities.KnowledgeBases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class LoreEntryConfiguration : IEntityTypeConfiguration<LoreEntry>
{
    public void Configure(EntityTypeBuilder<LoreEntry> builder)
    {
        builder.ToTable("LoreEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);

        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.IsEnabled).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.KnowledgeBaseId);
        builder.HasIndex(x => x.IsEnabled);
    }
}
