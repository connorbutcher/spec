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
            },
            new Permission
            {
                Id = 3,
                Key = PermissionKeys.TemplatesManage,
                Description = "Create, change and delete table templates and everything in them.",
            },
            new Permission
            {
                Id = 4,
                Key = PermissionKeys.CellTypesManage,
                Description = "Create, change and delete cell types.",
            },
            new Permission
            {
                Id = 5,
                Key = PermissionKeys.SheetsEdit,
                Description = "Change a sheet: its tables, sections, rows, column blocks and cell values.",
            },
            new Permission
            {
                Id = 6,
                Key = PermissionKeys.SheetsPublish,
                Description = "Publish a sheet's drafts as its next version.",
            });
    }
}
