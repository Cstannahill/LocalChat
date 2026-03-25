using LocalChat.Domain.Entities.Audio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class SpeechClipConfiguration : IEntityTypeConfiguration<SpeechClip>
{
    public void Configure(EntityTypeBuilder<SpeechClip> builder)
    {
        builder.ToTable("SpeechClips");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Voice)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ModelIdentifier)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ResponseFormat)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RelativeUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.SourceText)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.AgentId);

        builder.HasOne(x => x.Agent)
            .WithMany()
            .HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Message)
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
