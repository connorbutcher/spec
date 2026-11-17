using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Values;

internal class TableWidgetValueConfiguration : IEntityTypeConfiguration<DbTableWidgetValue>
{
    public void Configure(EntityTypeBuilder<DbTableWidgetValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableWidgetValues", "values");

        builder.Property(x => x.Id)
            .HasColumnName("TableWidgetValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableWidgetId)
            .HasColumnName("SpecSheetTableWidgetID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<WidgetValue>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
