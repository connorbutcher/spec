using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PUSpecSheet.Database.Configurations.Users;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<DbRolePermission>
{
    public void Configure(EntityTypeBuilder<DbRolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(link => new { link.RoleId, link.PermissionId });

        builder.HasOne(link => link.Role)
            .WithMany(role => role.Permissions)
            .HasForeignKey(link => link.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(link => link.Permission)
            .WithMany(permission => permission.Roles)
            .HasForeignKey(link => link.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
