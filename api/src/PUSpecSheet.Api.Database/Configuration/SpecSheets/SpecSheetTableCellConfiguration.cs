using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.SpecSheets;

internal class SpecSheetTableCellConfiguration : IEntityTypeConfiguration<DbSpecSheetTableCell>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTableCell> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTableCells", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTableCellID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableRowId)
            .HasColumnName("SpecSheetTableRowID")
            .HasColumnType("int");

        builder.Property(x => x.TableCellTypeId)
            .HasColumnName("TableCellTypeId")
            .HasColumnType("tinyint");

        builder.Property(x => x.CellConfigurationId)
            .HasColumnName("CellConfigurationID")
            .HasColumnType("int");

        builder.Property(x => x.RowNumber)
            .HasColumnType("tinyint");

        builder.Property(x => x.ColumnNumber)
            .HasColumnType("smallint");

        builder.Property(x => x.ColSpan)
            .HasColumnType("tinyint");

        builder.Property(x => x.RowSpan)
            .HasColumnType("tinyint");

        builder.Property(x => x.ReferenceId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("CreatedByUserID")
            .HasColumnType("smallint");

        builder.Property(x => x.LastModifiedByUserId)
            .HasColumnName("LastModifiedByUserID")
            .HasColumnType("smallint");

        builder.HasOne(builder => builder.TableCellType)
            .WithMany()
            .HasForeignKey(builder => builder.TableCellTypeId);

        builder.HasOne(builder => builder.CellConfiguration)
            .WithMany()
            .HasForeignKey(builder => builder.CellConfigurationId);

        builder.HasOne(builder => builder.CreatedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.CreatedByUserId);

        builder.HasOne(builder => builder.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(builder => builder.LastModifiedByUserId);

        builder.HasOne(builder => builder.NumericValue)
            .WithOne()
            .HasForeignKey<DbNumericValue>(builder => builder.SpecSheetTableCellId);

        builder.HasOne(builder => builder.TextValue)
            .WithOne()
            .HasForeignKey<DbTextValue>(builder => builder.SpecSheetTableCellId);

        builder.HasOne(builder => builder.PairValue)
            .WithOne()
            .HasForeignKey<DbPairValue>(builder => builder.SpecSheetTableCellId);

        builder.HasOne(builder => builder.LinkedSpecSheetValue)
            .WithOne()
            .HasForeignKey<DbLinkedSpecSheetValue>(builder => builder.SpecSheetTableCellId);

        builder.Property(x => x.ConfigurationOverride)
            .HasColumnName("ConfigurationOverride")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<ICellProperties>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
