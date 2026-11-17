using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetPropertyConfiguration : IEntityTypeConfiguration<DbSheetProperty>
{
    public void Configure(EntityTypeBuilder<DbSheetProperty> builder)
    {
        builder.ToTable("SheetProperties", DatabaseSchemas.Spec);

        builder.HasKey(property => property.Id);

        builder.HasPublicId(property => property.PublicId);

        builder.Property(property => property.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(property => new { property.SheetId, property.SheetTypePropertyId })
            .IsUnique();

        builder.HasOne(property => property.Sheet)
            .WithMany(sheet => sheet.Properties)
            .HasForeignKey(property => property.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(property => property.SheetTypeProperty)
            .WithMany()
            .HasForeignKey(property => property.SheetTypePropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
