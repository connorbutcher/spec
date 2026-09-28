using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(user => user.UserName)
            .IsUnique();

        builder.Property(user => user.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(user => user.Email)
            .HasMaxLength(256);

        builder.Property(user => user.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

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
