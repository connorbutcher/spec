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

        // One cell per template cell in each row.
        builder.HasIndex(cell => new { cell.SheetRowId, cell.TemplateCellId })
            .IsUnique();

        builder.HasOne(cell => cell.SheetRow)
            .WithMany(row => row.Cells)
            .HasForeignKey(cell => cell.SheetRowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cell => cell.TemplateCell)
            .WithMany()
            .HasForeignKey(cell => cell.TemplateCellId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
