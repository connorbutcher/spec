using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetRowRevisionConfiguration : IEntityTypeConfiguration<SheetRowRevision>
{
    public void Configure(EntityTypeBuilder<SheetRowRevision> builder)
    {
        builder.ToTable("SheetRowRevisions", table =>
        {
            // A published revision always has a publish time; a draft never does.
            table.HasCheckConstraint(
                "CK_SheetRowRevisions_PublishedAt",
                "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
        });

        builder.HasKey(revision => revision.Id);

        builder.HasIndex(revision => new { revision.SheetRowId, revision.RevisionNumber })
            .IsUnique();

        // The row lock: at most one draft per row, enforced by the database.
        builder.HasIndex(revision => revision.SheetRowId)
            .IsUnique()
            .HasFilter("[Status] = 0")
            .HasDatabaseName("UX_SheetRowRevisions_OneDraftPerRow");

        // "As of a date": each row's latest revision published at or before a moment.
        builder.HasIndex(revision => new { revision.SheetRowId, revision.PublishedAtUtc })
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_SheetRowRevisions_Published");

        // Finding the rows a user has locked.
        builder.HasIndex(revision => new { revision.AuthorUserId, revision.Status });

        builder.Property(revision => revision.Status)
            .HasConversion<int>();

        builder.Property(revision => revision.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(revision => revision.UpdatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(revision => revision.SheetRow)
            .WithMany(row => row.Revisions)
            .HasForeignKey(revision => revision.SheetRowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions and rows both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.RowRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
