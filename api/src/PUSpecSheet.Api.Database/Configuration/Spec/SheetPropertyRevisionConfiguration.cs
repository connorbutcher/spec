using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetPropertyRevisionConfiguration : IEntityTypeConfiguration<DbSheetPropertyRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetPropertyRevision> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetPropertyRevisions", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetPropertyRevisionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetPropertyId)
            .HasColumnName("SheetPropertyID")
            .HasColumnType("int");

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.RevisionNumber)
            .HasColumnType("int");

        builder.Property(x => x.Status)
            .HasColumnType("tinyint");

        builder.Property(x => x.PropertyValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsDeleted)
            .HasColumnType("bit");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.Property(x => x.LastModifiedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.Property(x => x.PublishedDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.SupersededDateTime)
            .HasColumnType("datetime2(7)");

        builder.Property(x => x.SheetVersionId)
            .HasColumnName("SheetVersionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetSubmissionId)
            .HasColumnName("SheetSubmissionID")
            .HasColumnType("int");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheet>()
            .WithMany()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheetVersion>()
            .WithMany()
            .HasForeignKey(x => x.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheetSubmission>()
            .WithMany()
            .HasForeignKey(x => x.SheetSubmissionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
