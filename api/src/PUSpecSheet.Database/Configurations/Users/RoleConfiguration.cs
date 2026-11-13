using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Configurations.Users;

public sealed class RoleConfiguration : IEntityTypeConfiguration<DbRole>
{
    public void Configure(EntityTypeBuilder<DbRole> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.Property(role => role.Description)
            .HasMaxLength(500);
    }
}
