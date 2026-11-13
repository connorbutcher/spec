using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.Database.Configurations.Approvals;

public sealed class SheetSubmissionConfiguration : IEntityTypeConfiguration<DbSheetSubmission>
{
    public void Configure(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        builder.ToTable("SheetSubmissions", DatabaseSchemas.Approval);

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
