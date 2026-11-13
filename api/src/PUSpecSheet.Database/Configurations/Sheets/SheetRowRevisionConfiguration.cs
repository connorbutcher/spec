using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetRowRevisionConfiguration : IEntityTypeConfiguration<DbSheetRowRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetRowRevision> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetRowRevision> builder)
    {
        builder.ToTable("SheetRowRevisions", table =>
        {
            // Draft or published, nothing else: the queries for live revisions rely on there being only two.
            table.HasCheckConstraint("CK_SheetRowRevisions_Status", "[Status] IN (0, 1)");

            // An item's first revision is number 1.
            table.HasCheckConstraint("CK_SheetRowRevisions_RevisionNumber", "[RevisionNumber] >= 1");

            // A published revision always has a publish time; a draft never does.
            table.HasCheckConstraint(
                "CK_SheetRowRevisions_PublishedAt",
                "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");

            // Only a published revision can be superseded, and never before it was published.
            table.HasCheckConstraint(
                "CK_SheetRowRevisions_SupersededAt",
                "[SupersededAtUtc] IS NULL OR ([Status] = 1 AND [SupersededAtUtc] >= [PublishedAtUtc])");
        });

        builder.HasKey(revision => revision.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetRowRevision> builder)
    {
        builder.Property(revision => revision.RowVersion)
            .IsRowVersion();

        builder.Property(revision => revision.Status)
            .HasConversion<int>();

        builder.Property(revision => revision.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(revision => revision.UpdatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetRowRevision> builder)
    {
        // The current state: at most one published, not-yet-superseded revision per row.
        builder.HasIndex(revision => revision.SheetRowId, "UX_SheetRowRevisions_OneCurrentPerRow")
            .IsUnique()
            .HasFilter("[Status] = 1 AND [SupersededAtUtc] IS NULL");

        builder.HasIndex(revision => new { revision.SheetRowId, revision.RevisionNumber })
            .IsUnique();

        // The row lock: at most one draft per row, enforced by the database.
        builder.HasIndex(revision => revision.SheetRowId, "UX_SheetRowRevisions_OneDraftPerRow")
            .IsUnique()
            .HasFilter("[Status] = 0");

        // "As of" a moment: the revision whose [PublishedAtUtc, SupersededAtUtc) range covers it.
        builder.HasIndex(revision => new { revision.SheetRowId, revision.PublishedAtUtc })
            .IncludeProperties(revision => revision.SupersededAtUtc)
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_SheetRowRevisions_Published");

        // Finding the rows a user has locked.
        builder.HasIndex(revision => new { revision.AuthorUserId, revision.Status });

        // "This sheet at a moment", for the published API and past views: one range seek on the sheet, whose
        // cost follows this sheet's history rather than every sheet's.
        builder.HasIndex(revision => new { revision.SheetId, revision.PublishedAtUtc })
            .IncludeProperties(revision => new { revision.SupersededAtUtc, revision.IsDeleted, revision.DisplayOrder, revision.SheetRowId })
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_SheetRowRevisions_SheetPublished");

        // The live sheet: its items' current revisions (published, not superseded) and their drafts.
        builder.HasIndex(revision => new { revision.SheetId, revision.Status })
            .IncludeProperties(revision => new { revision.SupersededAtUtc, revision.SheetRowId, revision.AuthorUserId });
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetRowRevision> builder)
    {
        // Copied from the item's sheet. Items already reach the sheet through their parents, so this can't cascade as well.
        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetRow)
            .WithMany(row => row.Revisions)
            .HasForeignKey(revision => revision.SheetRowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions and rows both sit under the sheet, so this can't cascade as well.
        // The publish awaiting approval the draft is part of. Submissions sit under the sheet too, so no cascade.
        builder.HasOne<DbSheetSubmission>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetSubmissionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.RowRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
