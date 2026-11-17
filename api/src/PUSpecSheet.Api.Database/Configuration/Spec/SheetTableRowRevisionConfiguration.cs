using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableRowRevisionConfiguration : IEntityTypeConfiguration<DbSheetTableRowRevision>
{
    public void Configure(EntityTypeBuilder<DbSheetTableRowRevision> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetTableRowRevisions", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetTableRowRevisionID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetTableRowId)
            .HasColumnName("SheetTableRowID")
            .HasColumnType("int");

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.RevisionNumber)
            .HasColumnType("int");

        builder.Property(x => x.Status)
            .HasColumnType("tinyint");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("int");

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

        builder.HasOne(x => x.SheetVersion)
            .WithMany()
            .HasForeignKey(x => x.SheetVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSheetSubmission>()
            .WithMany()
            .HasForeignKey(x => x.SheetSubmissionId)
            .OnDelete(DeleteBehavior.NoAction);


        builder.HasMany(x => x.TextValues)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.NumericValues)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PairValues)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.LinkedSheetValues)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CellConfigurationOverrides)
            .WithOne()
            .HasForeignKey(x => x.SheetTableRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
