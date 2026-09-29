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

        // Only one cell can start at a given column in a row.
        builder.HasIndex(cell => new { cell.TemplateRowId, cell.Column })
            .IsUnique();

        builder.HasOne(cell => cell.TemplateRow)
            .WithMany(row => row.Cells)
            .HasForeignKey(cell => cell.TemplateRowId)
            .OnDelete(DeleteBehavior.Cascade);

        // A cell type that is still used by a cell can't be deleted.
        builder.HasOne(cell => cell.CellType)
            .WithMany()
            .HasForeignKey(cell => cell.CellTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
