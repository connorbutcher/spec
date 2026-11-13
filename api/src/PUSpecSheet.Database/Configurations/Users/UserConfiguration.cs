using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PUSpecSheet.Database.Configurations.Users;

public sealed class UserConfiguration : IEntityTypeConfiguration<DbUser>
{
    public void Configure(EntityTypeBuilder<DbUser> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbUser> builder)
    {
        builder.Property(user => user.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(user => user.Email)
            .HasMaxLength(256);

        builder.Property(user => user.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbUser> builder)
    {
        builder.HasIndex(user => user.UserName)
            .IsUnique();
    }
}
