using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Configuration.Access;

public class PermissionConfiguration : IEntityTypeConfiguration<DbPermission>
{
    public void Configure(EntityTypeBuilder<DbPermission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("Permissions", "access");

        builder.Property(x => x.Id)
            .HasColumnName("PermissionID")
            .HasColumnType("smallint")
            .UseIdentityColumn();

        builder.Property(x => x.Title)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.Description)
            .HasColumnType("varchar(512)");

        builder.Property(x => x.Key)
            .HasColumnType("varchar(64)");
    }
}
