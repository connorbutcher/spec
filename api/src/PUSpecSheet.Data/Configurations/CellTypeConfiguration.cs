using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Data.Conversions;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

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

        builder.Property(cellType => cellType.Configuration)
            .IsRequired()
            .HasConversion<CellConfigurationConverter>();

        builder.Property(cellType => cellType.Style)
            .IsRequired()
            .HasConversion<CellStyleConverter>();

        // Starting cell types, one per kind plus a sample dropdown. Users can edit or add to these.
        builder.HasData(
            new CellType
            {
                Id = CellTypeSeedIds.Heading,
                Name = "Heading",
                Kind = CellKind.Heading,
                DisplayOrder = 1,
                Description = "Fixed text such as a column header.",
                Configuration = new HeadingCellConfiguration(),
                Style = new CellStyle { Bold = true },
            },
            new CellType
            {
                Id = CellTypeSeedIds.Text,
                Name = "Text",
                Kind = CellKind.Text,
                DisplayOrder = 2,
                Configuration = new TextCellConfiguration { MaxLength = 200 },
            },
            new CellType
            {
                Id = 3,
                Name = "Number",
                Kind = CellKind.Number,
                DisplayOrder = 3,
                Configuration = new NumberCellConfiguration { DecimalPlaces = 2 },
                Style = new CellStyle { Align = CellTextAlign.Right },
            },
            new CellType
            {
                Id = 4,
                Name = "Date",
                Kind = CellKind.Date,
                DisplayOrder = 4,
                Configuration = new DateCellConfiguration(),
            },
            new CellType
            {
                Id = 5,
                Name = "Checkbox",
                Kind = CellKind.Checkbox,
                DisplayOrder = 5,
                Configuration = new CheckboxCellConfiguration(),
                Style = new CellStyle { Align = CellTextAlign.Center },
            },
            new CellType
            {
                Id = CellTypeSeedIds.PassFail,
                Name = "Pass / Fail",
                Kind = CellKind.TextDropdown,
                DisplayOrder = 6,
                Configuration = new TextDropdownCellConfiguration(),
            },
            new CellType
            {
                Id = CellTypeSeedIds.Group,
                Name = "Group",
                Kind = CellKind.Group,
                DisplayOrder = 7,
                Description = "A caption that groups the cells around it.",
                Configuration = new GroupCellConfiguration(),
                Style = new CellStyle { Bold = true, BackgroundColor = "#f1f5f9" },
            });
    }
}
