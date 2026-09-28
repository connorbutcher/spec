using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetSectionRevisionConfiguration : IEntityTypeConfiguration<SheetSectionRevision>
{
    public void Configure(EntityTypeBuilder<SheetSectionRevision> builder)
    {
        builder.ToTable("SheetSectionRevisions", table =>
        {
            // A published revision always has a publish time; a draft never does.
            table.HasCheckConstraint(
                "CK_SheetSectionRevisions_PublishedAt",
                "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
        });

        builder.HasKey(revision => revision.Id);

        builder.HasIndex(revision => new { revision.SheetSectionId, revision.RevisionNumber })
            .IsUnique();

        // The section lock: at most one draft per section, enforced by the database.
        builder.HasIndex(revision => revision.SheetSectionId)
            .IsUnique()
            .HasFilter("[Status] = 0")
            .HasDatabaseName("UX_SheetSectionRevisions_OneDraftPerSection");

        // "As of a date": each section's latest revision published at or before a moment.
        builder.HasIndex(revision => new { revision.SheetSectionId, revision.PublishedAtUtc })
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_SheetSectionRevisions_Published");

        builder.HasIndex(revision => new { revision.AuthorUserId, revision.Status });

        builder.Property(revision => revision.Status)
            .HasConversion<int>();

        builder.Property(revision => revision.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(revision => revision.UpdatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(revision => revision.SheetSection)
            .WithMany(section => section.Revisions)
            .HasForeignKey(revision => revision.SheetSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions and sections both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.SectionRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
