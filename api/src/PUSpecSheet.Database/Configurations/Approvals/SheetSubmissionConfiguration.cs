using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PUSpecSheet.Database.Configurations.Approvals;

public sealed class SheetSubmissionConfiguration : IEntityTypeConfiguration<DbSheetSubmission>
{
    public void Configure(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        builder.ToTable("SheetSubmissions", table =>
        {
            table.HasCheckConstraint("CK_SheetSubmissions_Status", "[Status] IN (0, 1, 2, 3)");

            // Only a decided submission has a decision time, and only an approved one has a version.
            table.HasCheckConstraint(
                "CK_SheetSubmissions_DecidedAt",
                "([Status] = 0 AND [DecidedAtUtc] IS NULL) OR ([Status] <> 0 AND [DecidedAtUtc] IS NOT NULL)");
            table.HasCheckConstraint(
                "CK_SheetSubmissions_Version",
                "[SheetVersionId] IS NULL OR [Status] = 1");
        });

        builder.HasKey(submission => submission.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        builder.Property(submission => submission.RowVersion)
            .IsRowVersion();

        builder.Property(submission => submission.Status)
            .HasConversion<int>();

        builder.Property(submission => submission.Note)
            .HasMaxLength(1000);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        // A sheet's pending submissions, and a submitter's own.
        builder.HasIndex(submission => new { submission.SheetId, submission.Status });
        builder.HasIndex(submission => new { submission.SubmittedByUserId, submission.Status });
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        builder.HasOne(submission => submission.Sheet)
            .WithMany()
            .HasForeignKey(submission => submission.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(submission => submission.SubmittedBy)
            .WithMany()
            .HasForeignKey(submission => submission.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions also sit under the sheet, so this can't cascade as well.
        builder.HasOne(submission => submission.SheetVersion)
            .WithMany()
            .HasForeignKey(submission => submission.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
