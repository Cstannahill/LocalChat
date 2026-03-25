using LocalChat.Domain.Entities.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class SceneStateExtractionEventConfiguration : IEntityTypeConfiguration<SceneStateExtractionEvent>
{
    public void Configure(EntityTypeBuilder<SceneStateExtractionEvent> builder)
    {
        builder.ToTable("SceneStateExtractionEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SlotFamily)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.SlotKey)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.CandidateContent)
            .IsRequired();

        builder.Property(x => x.CandidateNormalizedKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ReplacedMemoryContent)
            .IsRequired(false);

        builder.Property(x => x.Notes)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => x.SlotFamily);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.CreatedAt);
    }
}
