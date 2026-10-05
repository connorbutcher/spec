using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Data.Conversions;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TemplateCellConfiguration : IEntityTypeConfiguration<TemplateCell>
{
    public void Configure(EntityTypeBuilder<TemplateCell> builder)
    {
        builder.ToTable("TemplateCells", table =>
        {
            table.HasCheckConstraint("CK_TemplateCells_Column", "[Column] >= 1");
            table.HasCheckConstraint("CK_TemplateCells_RowSpan", "[RowSpan] >= 1");
            table.HasCheckConstraint("CK_TemplateCells_ColumnSpan", "[ColumnSpan] >= 1");
        });

        builder.HasKey(cell => cell.Id);

        builder.Property(cell => cell.RowSpan)
            .HasDefaultValue(1);

        builder.Property(cell => cell.ColumnSpan)
            .HasDefaultValue(1);

        builder.Property(cell => cell.Caption)
            .HasMaxLength(200);

        builder.Property(cell => cell.ConfigurationOverride)
            .HasConversion<CellConfigurationConverter>();

        builder.Property(cell => cell.StyleOverride)
            .HasConversion<CellStyleConverter>();

        builder.Property(cell => cell.LookupKey)
            .HasMaxLength(LookupKeyRules.MaxLength);

        // Finding the cells that answer to a lookup key; most cells have none.
        builder.HasIndex(cell => cell.LookupKey)
            .HasFilter("[LookupKey] IS NOT NULL");

        // Only one cell can start at a given column of a row's own cells, or of one column block in a row.
        // No filter, so the row's own cells (no block) are unique among themselves too.
        builder.HasIndex(cell => new { cell.TemplateRowId, cell.TemplateColumnBlockId, cell.Column })
            .IsUnique()
            .HasFilter(null);

        builder.HasOne(cell => cell.TemplateRow)
            .WithMany(row => row.Cells)
            .HasForeignKey(cell => cell.TemplateRowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cells already cascade from their rows, and SQL Server allows only one cascade path, so a block's
        // cells are deleted before the block.
        builder.HasOne(cell => cell.TemplateColumnBlock)
            .WithMany(block => block.Cells)
            .HasForeignKey(cell => cell.TemplateColumnBlockId)
            .OnDelete(DeleteBehavior.NoAction);

        // A cell type that is still used by a cell can't be deleted.
        builder.HasOne(cell => cell.CellType)
            .WithMany()
            .HasForeignKey(cell => cell.CellTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
