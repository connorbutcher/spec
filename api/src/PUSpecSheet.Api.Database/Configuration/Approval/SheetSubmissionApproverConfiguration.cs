using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Approval;

namespace PUSpecSheet.Api.Database.Configuration.Approval;

internal class SheetSubmissionApproverConfiguration : IEntityTypeConfiguration<DbSheetSubmissionApprover>
{
    public void Configure(EntityTypeBuilder<DbSheetSubmissionApprover> builder)
    {
        builder.HasKey(x => new { x.SheetSubmissionId, x.UserId });
        builder.ToTable("SheetSubmissionApprovers", "approval");

        builder.Property(x => x.SheetSubmissionId)
            .HasColumnName("SheetSubmissionID")
            .HasColumnType("int");

        builder.Property(x => x.UserId)
            .HasColumnName("UserID")
            .HasColumnType("smallint");

        builder.Property(x => x.Decision)
            .HasColumnType("tinyint");

        builder.Property(x => x.DecidedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.Reason)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
