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

        builder.HasOne(property => property.Sheet)
            .WithMany(sheet => sheet.Properties)
            .HasForeignKey(property => property.SheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
