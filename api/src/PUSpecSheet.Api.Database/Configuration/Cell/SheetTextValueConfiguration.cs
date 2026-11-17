using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Cell;

namespace PUSpecSheet.Api.Database.Configuration.Cell;

internal class SheetTextValueConfiguration : IEntityTypeConfiguration<DbSheetTextValue>
{
    public void Configure(EntityTypeBuilder<DbSheetTextValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableRowRevisionId, x.SheetTableCellId });
        builder.ToTable("TextValues", "cell");

        builder.Property(x => x.SheetTableRowRevisionId)
            .HasColumnName("SheetTableRowRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableCellId)
            .HasColumnName("SheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.SheetTableCell)
            .WithMany()
            .HasForeignKey(x => x.SheetTableCellId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
