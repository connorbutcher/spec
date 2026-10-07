using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TableTemplateVersionConfiguration : IEntityTypeConfiguration<TableTemplateVersion>
{
    public void Configure(EntityTypeBuilder<TableTemplateVersion> builder)
    {
        builder.ToTable("TableTemplateVersions", table =>
        {
            table.HasCheckConstraint("CK_TableTemplateVersions_VersionNumber", "[VersionNumber] >= 1");
            table.HasCheckConstraint("CK_TableTemplateVersions_StickyColumnCount", "[StickyColumnCount] >= 0");
        });

        builder.HasKey(version => version.Id);

        builder.Property(version => version.StickyColumnCount)
            .HasDefaultValue(0);

        builder.Property(version => version.Orientation)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(version => version.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(version => new { version.TableTemplateId, version.VersionNumber })
            .IsUnique();

        // Deleting a template removes its versions; a version a sheet uses blocks that (see SheetTable).
        builder.HasOne(version => version.TableTemplate)
            .WithMany(template => template.Versions)
            .HasForeignKey(version => version.TableTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
