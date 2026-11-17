using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Cell;

namespace PUSpecSheet.Api.Database.Configuration.Cell;

internal class SheetPairValueConfiguration : IEntityTypeConfiguration<DbSheetPairValue>
{
    public void Configure(EntityTypeBuilder<DbSheetPairValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableRowRevisionId, x.SheetTableCellId });
        builder.ToTable("PairValues", "cell");

        builder.Property(x => x.SheetTableRowRevisionId)
            .HasColumnName("SheetTableRowRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableCellId)
            .HasColumnName("SheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.PairValueX)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.PairValueY)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.SheetTableCell)
            .WithMany()
            .HasForeignKey(x => x.SheetTableCellId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
