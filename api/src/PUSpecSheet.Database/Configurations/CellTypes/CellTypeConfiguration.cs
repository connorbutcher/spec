using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Conversions;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.Database.Configurations.CellTypes;

public sealed class CellTypeConfiguration : IEntityTypeConfiguration<DbCellType>
{
    public void Configure(EntityTypeBuilder<DbCellType> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbCellType> builder)
    {
        builder.ToTable("CellTypes", DatabaseSchemas.Design);

        builder.HasKey(cellType => cellType.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbCellType> builder)
    {
        builder.Property(cellType => cellType.Name)
            .IsRequired()
            .HasMaxLength(100);

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
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbCellType> builder)
    {
        builder.HasIndex(cellType => cellType.Name)
            .IsUnique();
    }
}
