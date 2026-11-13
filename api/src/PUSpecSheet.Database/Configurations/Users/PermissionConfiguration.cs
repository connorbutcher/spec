using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Configurations.Users;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<DbPermission>
{
    public void Configure(EntityTypeBuilder<DbPermission> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbPermission> builder)
    {
        builder.ToTable("Permissions", DatabaseSchemas.Auth);

        builder.HasKey(permission => permission.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbPermission> builder)
    {
        builder.Property(permission => permission.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(permission => permission.Description)
            .IsRequired()
            .HasMaxLength(500);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbPermission> builder)
    {
        builder.HasIndex(permission => permission.Key)
            .IsUnique();
    }
}
