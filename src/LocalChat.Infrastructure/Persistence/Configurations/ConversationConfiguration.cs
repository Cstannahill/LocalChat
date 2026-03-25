using LocalChat.Domain.Entities.Conversations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.DirectorInstructions)
            .IsRequired(false);

        builder.Property(x => x.DirectorInstructionsUpdatedAt)
            .IsRequired(false);

        builder.Property(x => x.SceneContext)
            .IsRequired(false);

        builder.Property(x => x.SceneContextUpdatedAt)
            .IsRequired(false);

        builder.Property(x => x.IsOocModeEnabled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.CharacterId);
        builder.HasIndex(x => x.UserPersonaId);
        builder.HasIndex(x => x.RuntimeModelProfileOverrideId);
        builder.HasIndex(x => x.RuntimeGenerationPresetOverrideId);
        builder.HasIndex(x => x.ParentConversationId);
        builder.HasIndex(x => x.BranchedFromMessageId);
        builder.HasIndex(x => x.UpdatedAt);

        builder.HasOne(x => x.Character)
            .WithMany(x => x.Conversations)
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserPersona)
            .WithMany(x => x.Conversations)
            .HasForeignKey(x => x.UserPersonaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RuntimeModelProfileOverride)
            .WithMany()
            .HasForeignKey(x => x.RuntimeModelProfileOverrideId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RuntimeGenerationPresetOverride)
            .WithMany()
            .HasForeignKey(x => x.RuntimeGenerationPresetOverrideId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ParentConversation)
            .WithMany()
            .HasForeignKey(x => x.ParentConversationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SummaryCheckpoints)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
