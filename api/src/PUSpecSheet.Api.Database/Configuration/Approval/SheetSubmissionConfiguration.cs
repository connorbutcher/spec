using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Approval;

internal class SheetSubmissionConfiguration : IEntityTypeConfiguration<DbSheetSubmission>
{
    public void Configure(EntityTypeBuilder<DbSheetSubmission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetSubmissions", "approval");

        builder.Property(x => x.Id)
            .HasColumnName("SheetSubmissionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.Note)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SubmittedByUserId)
            .HasColumnName("SubmittedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.SubmittedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.Status)
            .HasColumnType("tinyint");

        builder.Property(x => x.DecidedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.SheetVersionId)
            .HasColumnName("SheetVersionID")
            .HasColumnType("int");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.SubmittedByUser)
            .WithMany()
            .HasForeignKey(x => x.SubmittedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Approvers)
            .WithOne()
            .HasForeignKey(x => x.SheetSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DbSheetVersion>()
            .WithMany()
            .HasForeignKey(x => x.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
