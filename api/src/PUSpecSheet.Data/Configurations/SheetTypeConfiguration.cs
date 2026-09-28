using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetTypeConfiguration : IEntityTypeConfiguration<SheetType>
{
    public void Configure(EntityTypeBuilder<SheetType> builder)
    {
        builder.ToTable("SheetTypes");

        builder.HasKey(sheetType => sheetType.Id);

        builder.Property(sheetType => sheetType.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(sheetType => sheetType.Name)
            .IsUnique();

        builder.HasData(
            new SheetType { Id = 1, Name = "Specification", DisplayOrder = 1 },
            new SheetType { Id = 2, Name = "Parts", DisplayOrder = 2 },
            new SheetType { Id = 3, Name = "PFKs", DisplayOrder = 3 },
            new SheetType { Id = 4, Name = "Engine Specifications", DisplayOrder = 4 },
            new SheetType { Id = 5, Name = "Confirmation", DisplayOrder = 5 },
            new SheetType { Id = 6, Name = "Torque Sheet", DisplayOrder = 6 });
    }
}
