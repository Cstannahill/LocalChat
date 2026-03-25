using LocalChat.Domain.Entities.Agents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class AgentSampleDialogueConfiguration
    : IEntityTypeConfiguration<AgentSampleDialogue>
{
    public void Configure(EntityTypeBuilder<AgentSampleDialogue> builder)
    {
        builder.ToTable("AgentSampleDialogues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserMessage).IsRequired();

        builder.Property(x => x.AssistantMessage).IsRequired();

        builder.Property(x => x.SortOrder).IsRequired();

        builder.HasIndex(x => new { x.AgentId, x.SortOrder }).IsUnique();
    }
}
