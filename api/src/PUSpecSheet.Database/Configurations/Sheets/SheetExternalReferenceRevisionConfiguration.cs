using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetExternalReferenceRevisionConfiguration : IEntityTypeConfiguration<DbSheetExternalReferenceRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetExternalReferenceRevision> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetExternalReferenceRevision> builder)
    {
        builder.ToTable("SheetExternalReferenceRevisions", DatabaseSchemas.Spec);

        builder.HasKey(revision => revision.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetExternalReferenceRevision> builder)
    {
        builder.Property(revision => revision.RowVersion)
            .IsRowVersion();

        builder.Property(revision => revision.PushedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(revision => revision.SystemName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(revision => revision.OwnerName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(revision => revision.Url)
            .IsRequired();

        builder.Property(revision => revision.Type)
            .IsRequired()
            .HasMaxLength(32);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetExternalReferenceRevision> builder)
    {
        // The current state: at most one not-yet-superseded revision per reference.
        builder.HasIndex(revision => revision.SheetExternalReferenceId, "UX_SheetExternalReferenceRevisions_OneCurrentPerReference")
            .IsUnique()
            .HasFilter("[SupersededAtUtc] IS NULL");

        builder.HasIndex(revision => new { revision.SheetExternalReferenceId, revision.RevisionNumber })
            .IsUnique();

        // One revision per version: pushing again for the same version updates it.
        builder.HasIndex(revision => new { revision.SheetExternalReferenceId, revision.SheetVersionId }, "UX_SheetExternalReferenceRevisions_OnePerVersion")
            .IsUnique();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetExternalReferenceRevision> builder)
    {
        // Copied from the reference's sheet. References already reach the sheet, so this can't cascade as well.
        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(revision => revision.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(revision => revision.SheetExternalReference)
            .WithMany(reference => reference.Revisions)
            .HasForeignKey(revision => revision.SheetExternalReferenceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Versions and references both sit under the sheet, so this can't cascade as well.
        builder.HasOne(revision => revision.SheetVersion)
            .WithMany(version => version.ExternalReferenceRevisions)
            .HasForeignKey(revision => revision.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
