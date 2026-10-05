using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
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
