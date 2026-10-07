using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetVersionConfiguration : IEntityTypeConfiguration<SheetVersion>
{
    public void Configure(EntityTypeBuilder<SheetVersion> builder)
    {
        builder.ToTable("SheetVersions", table =>
        {
            table.HasCheckConstraint("CK_SheetVersions_VersionNumber", "[VersionNumber] >= 1");
        });

        builder.HasKey(version => version.Id);

        builder.HasIndex(version => new { version.SheetId, version.VersionNumber })
            .IsUnique();

        // "As of a date" finds the latest version published at or before a moment.
        builder.HasIndex(version => new { version.SheetId, version.PublishedAtUtc });

        builder.Property(version => version.Note)
            .HasMaxLength(1000);

        builder.HasOne(version => version.Sheet)
            .WithMany(sheet => sheet.Versions)
            .HasForeignKey(version => version.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(version => version.PublishedBy)
            .WithMany()
            .HasForeignKey(version => version.PublishedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
