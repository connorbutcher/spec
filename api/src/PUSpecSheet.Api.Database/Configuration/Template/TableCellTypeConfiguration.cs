using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Styling;
using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Template;

internal class TableCellTypeConfiguration : IEntityTypeConfiguration<DbTableCellType>
{
    public void Configure(EntityTypeBuilder<DbTableCellType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableCellTypes", "template");

        builder.Property(x => x.Id)
            .HasColumnName("TableCellTypeID")
            .HasColumnType("tinyint")
            .UseIdentityColumn();

        builder.Property(x => x.Key)
            .HasColumnName("TableCellTypeKey")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.Title)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.Description)
            .HasColumnType("varchar(256)");

        builder.Property(x => x.Type)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.ModelType)
            .HasColumnType("varchar(32)");

        builder.Property(x => x.Styling)
            .HasColumnName("DefaultStyling")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<CellStyling>(v!, JsonSerializerSettings.DeserializerSettings) ?? CellStyling.Default);
    }
}