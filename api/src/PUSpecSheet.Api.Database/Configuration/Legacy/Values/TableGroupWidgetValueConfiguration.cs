using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.Values;

internal class TableGroupWidgetValueConfiguration : IEntityTypeConfiguration<DbTableGroupWidgetValue>
{
    public void Configure(EntityTypeBuilder<DbTableGroupWidgetValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TableGroupWidgetValues", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("TableGroupWidgetValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableGroupWidgetId)
            .HasColumnName("SpecSheetTableGroupWidgetID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<WidgetValue>(v, JsonSerializerSettings.DeserializerSettings));
    }
}
