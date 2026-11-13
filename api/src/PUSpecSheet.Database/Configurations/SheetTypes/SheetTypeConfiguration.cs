using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Configurations.SheetTypes;

public sealed class SheetTypeConfiguration : IEntityTypeConfiguration<DbSheetType>
{
    public void Configure(EntityTypeBuilder<DbSheetType> builder)
    {
        builder.ToTable("SheetTypes");

        builder.HasKey(sheetType => sheetType.Id);

        builder.Property(sheetType => sheetType.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(sheetType => sheetType.Name)
            .IsUnique();
    }
}
