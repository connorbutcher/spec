using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
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

        // The administrator role has no rows in RolePermissions: holding it implies every permission.
        builder.HasData(new Role
        {
            Id = WellKnownRoles.AdministratorId,
            Name = WellKnownRoles.AdministratorName,
            Description = "Can do everything. Every permission is implied, including ones added later.",
        });
    }
}
