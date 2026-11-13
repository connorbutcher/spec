using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetCellConfiguration : IEntityTypeConfiguration<DbSheetCell>
{
    public void Configure(EntityTypeBuilder<DbSheetCell> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbSheetCell> builder)
    {
        builder.ToTable("SheetCells", DatabaseSchemas.Spec);

        builder.HasKey(cell => cell.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbSheetCell> builder)
    {
        builder.HasPublicId(cell => cell.PublicId);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbSheetCell> builder)
    {
        // One cell per template cell in each row, and per column block copy for a block's cells. No
        // filter, so the row's own cells (no block) are unique too.
        builder.HasIndex(cell => new { cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId })
            .IsUnique()
            .HasFilter(null);
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbSheetCell> builder)
    {
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
