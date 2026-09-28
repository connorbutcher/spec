using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetRowConfiguration : IEntityTypeConfiguration<SheetRow>
{
    public void Configure(EntityTypeBuilder<SheetRow> builder)
    {
        builder.ToTable("SheetRows");

        builder.HasKey(row => row.Id);

        builder.HasIndex(row => new { row.SheetTableId, row.DisplayOrder });

        builder.Property(row => row.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(row => row.SheetTable)
            .WithMany(table => table.Rows)
            .HasForeignKey(row => row.SheetTableId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(row => row.TemplateRow)
            .WithMany()
            .HasForeignKey(row => row.TemplateRowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
