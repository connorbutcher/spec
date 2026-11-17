using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Configuration.Cell;

internal class SheetLinkedSheetValueConfiguration : IEntityTypeConfiguration<DbSheetLinkedSheetValue>
{
    public void Configure(EntityTypeBuilder<DbSheetLinkedSheetValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableRowRevisionId, x.SheetTableCellId });
        builder.ToTable("LinkedSheetValues", "cell");

        builder.Property(x => x.SheetTableRowRevisionId)
            .HasColumnName("SheetTableRowRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableCellId)
            .HasColumnName("SheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.LinkedSpecSheetTypeId)
            .HasColumnName("LinkedSpecSheetTypeID")
            .HasColumnType("tinyint");

        builder.Property(x => x.LinkedSpecSheetPhaseId)
            .HasColumnName("LinkedSpecSheetPhaseID")
            .HasColumnType("smallint");

        builder.Property(x => x.TableReferenceId)
            .HasColumnName("TableReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.LinkedReferenceId)
            .HasColumnName("LinkedReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.HasOne<DbSheetTableCell>()
            .WithMany()
            .HasForeignKey(x => x.SheetTableCellId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSpecSheetType>()
            .WithMany()
            .HasForeignKey(x => x.LinkedSpecSheetTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<DbSpecSheetPhase>()
            .WithMany()
            .HasForeignKey(x => x.LinkedSpecSheetPhaseId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
