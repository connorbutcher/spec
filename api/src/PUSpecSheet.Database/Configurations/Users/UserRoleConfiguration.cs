using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Configurations.Users;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<DbUserRole>
{
    public void Configure(EntityTypeBuilder<DbUserRole> builder)
    {
        builder.ToTable("UserRoles", DatabaseSchemas.Auth);

        builder.HasKey(link => new { link.UserId, link.RoleId });

        builder.HasOne(link => link.User)
            .WithMany(user => user.Roles)
            .HasForeignKey(link => link.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A role that users still hold can't be deleted.
        builder.HasOne(link => link.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(link => link.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
