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
        builder.ToTable("SheetRowRevisions", DatabaseSchemas.Spec);

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
