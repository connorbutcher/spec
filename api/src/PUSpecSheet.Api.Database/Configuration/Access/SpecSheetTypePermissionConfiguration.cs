using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Configuration.Access;

public class SpecSheetTypePermissionConfiguration : IEntityTypeConfiguration<DbSpecSheetTypePermission>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTypePermission> builder)
    {
        builder.HasKey(e => e.Id);
        builder.ToTable("SpecSheetTypePermissions", "access");

        builder.Property(e => e.Id)
            .HasColumnName("SpecSheetTypePermissionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(e => e.SpecSheetTypeId)
            .HasColumnName("SpecSheetTypeID")
            .HasColumnType("tinyint");

        builder.Property(e => e.PermissionId)
            .HasColumnName("PermissionID")
            .HasColumnType("smallint");

        builder.HasOne(e => e.SpecSheetType)
            .WithMany(e => e.Permissions)
            .HasForeignKey(e => e.SpecSheetTypeId);

        builder.HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId);
    }
}
