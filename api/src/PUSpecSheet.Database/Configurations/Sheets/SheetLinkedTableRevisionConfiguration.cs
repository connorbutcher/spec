using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetLinkedTableRevisionConfiguration : IEntityTypeConfiguration<DbSheetLinkedTableRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetLinkedTableRevision> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetLinkedTableRevision> builder)
    {
        builder.ToTable("SheetLinkedTableRevisions", DatabaseSchemas.Spec);

        builder.HasKey(revision => revision.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetLinkedTableRevision> builder)
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

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetLinkedTableRevision> builder)
    {
        // The current state: at most one published, not-yet-superseded revision per linked table.
        builder.HasIndex(revision => revision.SheetLinkedTableId, "UX_SheetLinkedTableRevisions_OneCurrentPerLinkedTable")
            .IsUnique()
            .HasFilter("[Status] = 1 AND [SupersededAtUtc] IS NULL");

        builder.HasIndex(revision => new { revision.SheetLinkedTableId, revision.RevisionNumber })
            .IsUnique();

        // The lock: at most one draft per linked table, enforced by the database.
        builder.HasIndex(revision => revision.SheetLinkedTableId, "UX_SheetLinkedTableRevisions_OneDraftPerLinkedTable")
            .IsUnique()
            .HasFilter("[Status] = 0");

    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetLinkedTableRevision> builder)
    {
        // Copied from the linked table's sheet. Linked tables already reach the sheet, so this can't cascade as well.
        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetLinkedTable)
            .WithMany(linked => linked.Revisions)
            .HasForeignKey(revision => revision.SheetLinkedTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // The publish awaiting approval the draft is part of. Submissions sit under the sheet too, so no cascade.
        builder.HasOne<DbSheetSubmission>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetSubmissionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Versions and linked tables both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.LinkedTableRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
