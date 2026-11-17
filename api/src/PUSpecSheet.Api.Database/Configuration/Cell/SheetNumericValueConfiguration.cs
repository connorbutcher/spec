using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Cell;

namespace PUSpecSheet.Api.Database.Configuration.Cell;

internal class SheetNumericValueConfiguration : IEntityTypeConfiguration<DbSheetNumericValue>
{
    public void Configure(EntityTypeBuilder<DbSheetNumericValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableRowRevisionId, x.SheetTableCellId });
        builder.ToTable("NumericValues", "cell");

        builder.Property(x => x.SheetTableRowRevisionId)
            .HasColumnName("SheetTableRowRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableCellId)
            .HasColumnName("SheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("decimal(18, 9)");

        builder.HasOne(x => x.SheetTableCell)
            .WithMany()
            .HasForeignKey(x => x.SheetTableCellId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
