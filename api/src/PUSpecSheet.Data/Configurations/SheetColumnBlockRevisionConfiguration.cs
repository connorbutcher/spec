using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetColumnBlockRevisionConfiguration : IEntityTypeConfiguration<SheetColumnBlockRevision>
{
    public void Configure(EntityTypeBuilder<SheetColumnBlockRevision> builder)
    {
        builder.ToTable("SheetColumnBlockRevisions", table =>
        {
            // A published revision always has a publish time; a draft never does.
            table.HasCheckConstraint(
                "CK_SheetColumnBlockRevisions_PublishedAt",
                "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");

            // Only a published revision can be superseded, and never before it was published.
            table.HasCheckConstraint(
                "CK_SheetColumnBlockRevisions_SupersededAt",
                "[SupersededAtUtc] IS NULL OR ([Status] = 1 AND [SupersededAtUtc] >= [PublishedAtUtc])");
        });

        builder.HasKey(revision => revision.Id);

        builder.Property(revision => revision.RowVersion)
            .IsRowVersion();

        // The current state: at most one published, not-yet-superseded revision per block.
        builder.HasIndex(revision => revision.SheetColumnBlockId, "UX_SheetColumnBlockRevisions_OneCurrentPerBlock")
            .IsUnique()
            .HasFilter("[Status] = 1 AND [SupersededAtUtc] IS NULL");

        builder.HasIndex(revision => new { revision.SheetColumnBlockId, revision.RevisionNumber })
            .IsUnique();

        // The block lock: at most one draft per block, enforced by the database.
        builder.HasIndex(revision => revision.SheetColumnBlockId, "UX_SheetColumnBlockRevisions_OneDraftPerBlock")
            .IsUnique()
            .HasFilter("[Status] = 0");

        // "As of" a moment: the revision whose [PublishedAtUtc, SupersededAtUtc) range covers it.
        builder.HasIndex(revision => new { revision.SheetColumnBlockId, revision.PublishedAtUtc })
            .IncludeProperties(revision => revision.SupersededAtUtc)
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_SheetColumnBlockRevisions_Published");

        builder.HasIndex(revision => new { revision.AuthorUserId, revision.Status });

        builder.Property(revision => revision.Status)
            .HasConversion<int>();

        builder.Property(revision => revision.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(revision => revision.UpdatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(revision => revision.SheetColumnBlock)
            .WithMany(block => block.Revisions)
            .HasForeignKey(revision => revision.SheetColumnBlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions and blocks both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.ColumnBlockRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
