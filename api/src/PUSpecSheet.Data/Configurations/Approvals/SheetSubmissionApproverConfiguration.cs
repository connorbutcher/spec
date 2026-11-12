using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Data.Configurations.Approvals;

public sealed class SheetSubmissionApproverConfiguration : IEntityTypeConfiguration<SheetSubmissionApprover>
{
    public void Configure(EntityTypeBuilder<SheetSubmissionApprover> builder)
    {
        builder.ToTable("SheetSubmissionApprovers", table =>
        {
            table.HasCheckConstraint("CK_SheetSubmissionApprovers_Decision", "[Decision] IN (0, 1, 2)");

            // A rejection always says why.
            table.HasCheckConstraint(
                "CK_SheetSubmissionApprovers_Reason",
                "[Decision] <> 2 OR (LEN([Reason]) > 0)");
        });

        builder.HasKey(approver => new { approver.SheetSubmissionId, approver.UserId });

        builder.Property(approver => approver.Decision)
            .HasConversion<int>();

        builder.Property(approver => approver.Reason)
            .HasMaxLength(1000);

        // What each person has waiting for them.
        builder.HasIndex(approver => new { approver.UserId, approver.Decision });

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
