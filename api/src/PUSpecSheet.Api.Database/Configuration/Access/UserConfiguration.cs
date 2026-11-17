using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Configuration.Access;

public class UserConfiguration : IEntityTypeConfiguration<DbUser>
{
    public void Configure(EntityTypeBuilder<DbUser> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("Users", "access");

        builder.Property(x => x.Id)
            .HasColumnName("UserID")
            .HasColumnType("smallint")
            .UseIdentityColumn();

        builder.Property(x => x.ObjectId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.DisplayName)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.EmailAddress)
            .HasMaxLength(256);

        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.TablePermissions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}
