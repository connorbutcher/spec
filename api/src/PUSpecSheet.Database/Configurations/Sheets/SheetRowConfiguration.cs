using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetRowConfiguration : IEntityTypeConfiguration<DbSheetRow>
{
    public void Configure(EntityTypeBuilder<DbSheetRow> builder)
    {
        builder.ToTable("SheetRows", DatabaseSchemas.Spec);

        builder.HasKey(row => row.Id);

        builder.HasPublicId(row => row.PublicId);

        builder.Property(row => row.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(row => row.SheetSection)
            .WithMany(section => section.Rows)
            .HasForeignKey(row => row.SheetSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(row => row.TemplateRow)
            .WithMany()
            .HasForeignKey(row => row.TemplateRowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
