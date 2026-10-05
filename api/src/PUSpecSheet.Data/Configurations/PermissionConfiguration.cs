using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(permission => permission.Key)
            .IsUnique();

        builder.Property(permission => permission.Description)
            .IsRequired()
            .HasMaxLength(500);

        // One row per key in PermissionKeys.
        builder.HasData(
            new Permission
            {
                Id = 1,
                Key = PermissionKeys.PhasesManage,
                Description = "Add phases and choose which sheet types each phase has.",
            },
            new Permission
            {
                Id = 2,
                Key = PermissionKeys.SheetTypesManage,
                Description = "Add sheet types.",
            });
    }
}
