using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetColumnBlockRevisionConfiguration : IEntityTypeConfiguration<DbSheetColumnBlockRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetColumnBlockRevision> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetColumnBlockRevision> builder)
    {
        builder.ToTable("SheetColumnBlockRevisions");

        builder.HasKey(revision => revision.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetColumnBlockRevision> builder)
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

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetColumnBlockRevision> builder)
    {
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
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetColumnBlockRevision> builder)
    {
        // Copied from the item's sheet. Items already reach the sheet through their parents, so this can't cascade as well.
        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetColumnBlock)
            .WithMany(block => block.Revisions)
            .HasForeignKey(revision => revision.SheetColumnBlockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions and blocks both sit under the sheet, so this can't cascade as well.
        // The publish awaiting approval the draft is part of. Submissions sit under the sheet too, so no cascade.
        builder.HasOne<DbSheetSubmission>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetSubmissionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.ColumnBlockRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
