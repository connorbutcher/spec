using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TemplateCellConfiguration : IEntityTypeConfiguration<TemplateCell>
{
    public void Configure(EntityTypeBuilder<TemplateCell> builder)
    {
        builder.ToTable("TemplateCells", table =>
        {
            table.HasCheckConstraint("CK_TemplateCells_Row", "[Row] >= 1");
            table.HasCheckConstraint("CK_TemplateCells_Column", "[Column] >= 1");
            table.HasCheckConstraint("CK_TemplateCells_RowSpan", "[RowSpan] >= 1");
            table.HasCheckConstraint("CK_TemplateCells_ColumnSpan", "[ColumnSpan] >= 1");
        });

        builder.HasKey(cell => cell.Id);

        builder.Property(cell => cell.RowSpan)
            .HasDefaultValue(1);

        builder.Property(cell => cell.ColumnSpan)
            .HasDefaultValue(1);

        // Only one cell can start at a given position in a section.
        builder.HasIndex(cell => new { cell.TemplateSectionId, cell.Row, cell.Column })
            .IsUnique();

        builder.HasOne(cell => cell.TemplateSection)
            .WithMany(section => section.Cells)
            .HasForeignKey(cell => cell.TemplateSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
