using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetCellConfiguration : IEntityTypeConfiguration<SheetCell>
{
    public void Configure(EntityTypeBuilder<SheetCell> builder)
    {
        builder.ToTable("SheetCells");

        builder.HasKey(cell => cell.Id);

        builder.HasPublicId(cell => cell.PublicId);

        // One cell per template cell in each row, and per column block copy for a block's cells. No
        // filter, so the row's own cells (no block) are unique too.
        builder.HasIndex(cell => new { cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId })
            .IsUnique()
            .HasFilter(null);

        builder.HasIndex(cell => cell.SheetColumnBlockId);

        builder.HasOne(cell => cell.SheetRow)
            .WithMany(row => row.Cells)
            .HasForeignKey(cell => cell.SheetRowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cells already cascade from their rows, and SQL Server allows only one cascade path, so a block's
        // cells are deleted before the block.
        builder.HasOne(cell => cell.SheetColumnBlock)
            .WithMany(block => block.Cells)
            .HasForeignKey(cell => cell.SheetColumnBlockId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(cell => cell.TemplateCell)
            .WithMany()
            .HasForeignKey(cell => cell.TemplateCellId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
