using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetTableConfiguration : IEntityTypeConfiguration<SheetTable>
{
    public void Configure(EntityTypeBuilder<SheetTable> builder)
    {
        builder.ToTable("SheetTables");

        builder.HasKey(table => table.Id);

        builder.HasPublicId(table => table.PublicId);

        builder.Property(table => table.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(table => table.Sheet)
            .WithMany(sheet => sheet.Tables)
            .HasForeignKey(table => table.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        // A template in use on a sheet can't be deleted.
        builder.HasOne(table => table.TableTemplate)
            .WithMany()
            .HasForeignKey(table => table.TableTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
