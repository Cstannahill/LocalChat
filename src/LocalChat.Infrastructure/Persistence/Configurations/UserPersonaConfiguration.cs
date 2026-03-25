using LocalChat.Domain.Entities.Personas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalChat.Infrastructure.Persistence.Configurations;

public sealed class UserPersonaConfiguration : IEntityTypeConfiguration<UserPersona>
{
    public void Configure(EntityTypeBuilder<UserPersona> builder)
    {
        builder.ToTable("UserPersonas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);

        builder.Property(x => x.Description).IsRequired();

        builder.Property(x => x.Traits).IsRequired();

        builder.Property(x => x.Preferences).IsRequired();

        builder.Property(x => x.AdditionalInstructions).IsRequired();

        builder.Property(x => x.IsDefault).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsDefault);
    }
}
