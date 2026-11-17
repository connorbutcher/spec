using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Configuration.Access;

internal class SpecSheetTablePermissionConfiguration : IEntityTypeConfiguration<DbSpecSheetTablePermission>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTablePermission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTablePermissions", "access");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTablePermissionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.UserId)
            .HasColumnName("UserID")
            .HasColumnType("smallint");

        builder.Property(x => x.TableReferenceId)
            .HasColumnName("SpecSheetTableReferenceID");
    }
}
