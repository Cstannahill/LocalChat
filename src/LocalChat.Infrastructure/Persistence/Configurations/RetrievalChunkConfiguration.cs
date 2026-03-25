using LocalChat.Domain.Entities.Retrieval;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class RetrievalChunkConfiguration : IEntityTypeConfiguration<RetrievalChunk>
{
    public void Configure(EntityTypeBuilder<RetrievalChunk> builder)
    {
        builder.ToTable("RetrievalChunks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceType).IsRequired().HasMaxLength(50);

        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.EmbeddingJson).IsRequired();

        builder.Property(x => x.IsEnabled).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.AgentId);
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => new { x.SourceType, x.SourceEntityId }).IsUnique();
    }
}
