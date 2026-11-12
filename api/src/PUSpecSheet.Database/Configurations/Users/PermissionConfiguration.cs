using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Entities.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Database.Configurations.Users;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureSeedData(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(permission => permission.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(permission => permission.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(permission => permission.Description)
            .IsRequired()
            .HasMaxLength(500);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(permission => permission.Key)
            .IsUnique();
    }

    private static void ConfigureSeedData(EntityTypeBuilder<Permission> builder)
    {
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
                Description = "Publish a sheet's drafts as its next version, once the chosen approvers approve.",
            },
            new Permission
            {
                Id = 7,
                Key = PermissionKeys.SheetsApprove,
                Description = "Approve or reject the changes someone else submitted for publishing.",
            });
    }
}
