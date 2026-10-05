using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

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

        // The developer user is an administrator, so everything keeps working until sign-in is added.
        builder.HasData(new UserRole
        {
            UserId = WellKnownUsers.DeveloperId,
            RoleId = WellKnownRoles.AdministratorId,
        });
    }
}
