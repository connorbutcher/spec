using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Configuration.Access;

public class UserPermissionConfiguration : IEntityTypeConfiguration<DbUserPermission>
{
    public void Configure(EntityTypeBuilder<DbUserPermission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("UserPermissions", "access");

        builder.Property(x => x.Id)
            .HasColumnName("UserPermissionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.UserId)
            .HasColumnName("UserID")
            .HasColumnType("smallint");

        builder.Property(x => x.PermissionId)
            .HasColumnName("PermissionID")
            .HasColumnType("smallint");

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.PermissionId);
    }
}
