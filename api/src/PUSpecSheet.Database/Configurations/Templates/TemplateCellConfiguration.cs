using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Conversions;
using PUSpecSheet.Database.Models.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TemplateCellConfiguration : IEntityTypeConfiguration<DbTemplateCell>
{
    public void Configure(EntityTypeBuilder<DbTemplateCell> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbTemplateCell> builder)
    {
        builder.ToTable("TemplateCells", DatabaseSchemas.Template);

        builder.HasKey(cell => cell.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbTemplateCell> builder)
    {
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
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbTemplateCell> builder)
    {
        // Only one cell can start at a given column of a row's own cells, or of one column block in a row.
        // No filter, so the row's own cells (no block) are unique among themselves too.
        builder.HasIndex(cell => new { cell.TemplateRowId, cell.TemplateColumnBlockId, cell.Column })
            .IsUnique()
            .HasFilter(null);
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbTemplateCell> builder)
    {
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
