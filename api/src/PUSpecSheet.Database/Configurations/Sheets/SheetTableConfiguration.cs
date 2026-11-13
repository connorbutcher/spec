using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetTableConfiguration : IEntityTypeConfiguration<DbSheetTable>
{
    public void Configure(EntityTypeBuilder<DbSheetTable> builder)
    {
        builder.ToTable("SheetTables", DatabaseSchemas.Spec);

        builder.HasKey(table => table.Id);

        builder.HasPublicId(table => table.PublicId);

        builder.Property(table => table.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(table => table.Sheet)
            .WithMany(sheet => sheet.Tables)
            .HasForeignKey(table => table.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        // A template version in use on a sheet can't be deleted.
        builder.HasOne(table => table.TableTemplateVersion)
            .WithMany()
            .HasForeignKey(table => table.TableTemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
