using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Data.Configurations;

public sealed class CellTypeConfiguration : IEntityTypeConfiguration<CellType>
{
    public void Configure(EntityTypeBuilder<CellType> builder)
    {
        builder.ToTable("CellTypes");

        builder.HasKey(cellType => cellType.Id);

        builder.Property(cellType => cellType.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(cellType => cellType.Name)
            .IsUnique();

        builder.Property(cellType => cellType.Kind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(cellType => cellType.Description)
            .HasMaxLength(500);

        builder.Property(cellType => cellType.MinValue)
            .HasPrecision(18, 4);

        builder.Property(cellType => cellType.MaxValue)
            .HasPrecision(18, 4);

        builder.Property(cellType => cellType.Unit)
            .HasMaxLength(20);

        // Starting cell types, one per kind plus a sample dropdown. Users can edit or add to these.
        builder.HasData(
            new CellType
            {
                Id = CellTypeSeedIds.Label,
                Name = "Label",
                Kind = CellKind.Label,
                DisplayOrder = 1,
                Description = "Fixed text such as a header.",
            },
            new CellType { Id = CellTypeSeedIds.Text, Name = "Text", Kind = CellKind.Text, DisplayOrder = 2, MaxLength = 200 },
            new CellType { Id = 3, Name = "Number", Kind = CellKind.Number, DisplayOrder = 3, DecimalPlaces = 2 },
            new CellType { Id = 4, Name = "Date", Kind = CellKind.Date, DisplayOrder = 4 },
            new CellType { Id = 5, Name = "Checkbox", Kind = CellKind.Checkbox, DisplayOrder = 5 },
            new CellType { Id = CellTypeSeedIds.PassFail, Name = "Pass / Fail", Kind = CellKind.Dropdown, DisplayOrder = 6 });
    }
}
