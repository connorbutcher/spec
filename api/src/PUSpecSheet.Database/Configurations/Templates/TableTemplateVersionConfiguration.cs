using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TableTemplateVersionConfiguration : IEntityTypeConfiguration<DbTableTemplateVersion>
{
    public void Configure(EntityTypeBuilder<DbTableTemplateVersion> builder)
    {
        builder.ToTable("TableTemplateVersions", DatabaseSchemas.Template);

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
