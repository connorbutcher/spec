using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetTableRevisionConfiguration : IEntityTypeConfiguration<SheetTableRevision>
{
    public void Configure(EntityTypeBuilder<SheetTableRevision> builder)
    {
        builder.ToTable("SheetTableRevisions", table =>
        {
            // A published revision always has a publish time; a draft never does.
            table.HasCheckConstraint(
                "CK_SheetTableRevisions_PublishedAt",
                "([Status] = 0 AND [PublishedAtUtc] IS NULL) OR ([Status] = 1 AND [PublishedAtUtc] IS NOT NULL)");
        });

        builder.HasKey(revision => revision.Id);

        builder.HasIndex(revision => new { revision.SheetTableId, revision.RevisionNumber })
            .IsUnique();

        // The table lock: at most one draft per table, enforced by the database.
        builder.HasIndex(revision => revision.SheetTableId)
            .IsUnique()
            .HasFilter("[Status] = 0")
            .HasDatabaseName("UX_SheetTableRevisions_OneDraftPerTable");

        // "As of a date": each table's latest revision published at or before a moment.
        builder.HasIndex(revision => new { revision.SheetTableId, revision.PublishedAtUtc })
            .HasFilter("[Status] = 1")
            .HasDatabaseName("IX_SheetTableRevisions_Published");

        builder.HasIndex(revision => new { revision.AuthorUserId, revision.Status });

        builder.Property(revision => revision.Status)
            .HasConversion<int>();

        builder.Property(revision => revision.Title)
            .HasMaxLength(200);

        builder.Property(revision => revision.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(revision => revision.UpdatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(revision => revision.SheetTable)
            .WithMany(table => table.Revisions)
            .HasForeignKey(revision => revision.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(revision => revision.Author)
            .WithMany()
            .HasForeignKey(revision => revision.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions and tables both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.TableRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
