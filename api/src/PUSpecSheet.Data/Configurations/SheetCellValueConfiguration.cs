using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetCellValueConfiguration : IEntityTypeConfiguration<SheetCellValue>
{
    public void Configure(EntityTypeBuilder<SheetCellValue> builder)
    {
        builder.ToTable("SheetCellValues");

        builder.HasKey(cell => cell.Id);

        builder.HasIndex(cell => new { cell.SheetRowRevisionId, cell.TemplateCellId })
            .IsUnique();

        builder.Property(cell => cell.Value)
            .HasMaxLength(4000);

        builder.HasOne(cell => cell.SheetRowRevision)
            .WithMany(revision => revision.CellValues)
            .HasForeignKey(cell => cell.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cell => cell.TemplateCell)
            .WithMany()
            .HasForeignKey(cell => cell.TemplateCellId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
