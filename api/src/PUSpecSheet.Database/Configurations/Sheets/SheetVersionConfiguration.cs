using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetVersionConfiguration : IEntityTypeConfiguration<DbSheetVersion>
{
    public void Configure(EntityTypeBuilder<DbSheetVersion> builder)
    {
        builder.ToTable("SheetVersions", DatabaseSchemas.Spec);

        builder.HasKey(version => version.Id);

        builder.HasIndex(version => new { version.SheetId, version.VersionNumber })
            .IsUnique();

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
