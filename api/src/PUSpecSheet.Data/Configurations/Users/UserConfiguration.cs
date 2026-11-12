using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Configurations.Users;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureSeedData(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<User> builder)
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

    private static void ConfigureIndexes(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(user => user.UserName)
            .IsUnique();
    }

    private static void ConfigureSeedData(EntityTypeBuilder<User> builder)
    {
        // Everything runs as this user until sign-in is added.
        builder.HasData(new User
        {
            Id = WellKnownUsers.DeveloperId,
            UserName = WellKnownUsers.DeveloperUserName,
            DisplayName = "Developer",
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Utc),
        });
    }
}
