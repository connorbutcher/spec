using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetPropertyRevisionConfiguration : IEntityTypeConfiguration<DbSheetPropertyRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetPropertyRevision> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetPropertyRevision> builder)
    {
        builder.ToTable("SheetPropertyRevisions", DatabaseSchemas.Spec);

        builder.HasKey(revision => revision.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetPropertyRevision> builder)
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

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetPropertyRevision> builder)
    {
        // The current state: at most one published, not-yet-superseded revision per property.
        builder.HasIndex(revision => revision.SheetPropertyId, "UX_SheetPropertyRevisions_OneCurrentPerProperty")
            .IsUnique()
            .HasFilter("[Status] = 1 AND [SupersededAtUtc] IS NULL");

        builder.HasIndex(revision => new { revision.SheetPropertyId, revision.RevisionNumber })
            .IsUnique();

        // The property lock: at most one draft per property, enforced by the database.
        builder.HasIndex(revision => revision.SheetPropertyId, "UX_SheetPropertyRevisions_OneDraftPerProperty")
            .IsUnique()
            .HasFilter("[Status] = 0");

        // A property changes at most once per version of its sheet.
        builder.HasIndex(revision => new { revision.SheetPropertyId, revision.SheetVersionId }, "UX_SheetPropertyRevisions_OnePerVersion")
            .IsUnique()
            .HasFilter("[SheetVersionId] IS NOT NULL");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetPropertyRevision> builder)
    {
        // Copied from the property's sheet. Properties already reach the sheet, so this can't cascade as well.
        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetProperty)
            .WithMany(property => property.Revisions)
            .HasForeignKey(revision => revision.SheetPropertyId)
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

        // Versions and properties both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.PropertyRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
