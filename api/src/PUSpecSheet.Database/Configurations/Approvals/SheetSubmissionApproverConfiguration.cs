using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.Database.Configurations.Approvals;

public sealed class SheetSubmissionApproverConfiguration : IEntityTypeConfiguration<DbSheetSubmissionApprover>
{
    public void Configure(EntityTypeBuilder<DbSheetSubmissionApprover> builder)
    {
        builder.ToTable("SheetSubmissionApprovers");

        builder.HasKey(approver => new { approver.SheetSubmissionId, approver.UserId });

        builder.Property(approver => approver.Decision)
            .HasConversion<int>();

        builder.Property(approver => approver.Reason)
            .HasMaxLength(1000);

        builder.HasOne(approver => approver.SheetSubmission)
            .WithMany(submission => submission.Approvers)
            .HasForeignKey(approver => approver.SheetSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(approver => approver.User)
            .WithMany()
            .HasForeignKey(approver => approver.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
