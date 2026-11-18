using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetTableGroupWidgetValueConfiguration : IEntityTypeConfiguration<DbSheetTableGroupWidgetValue>
{
    public void Configure(EntityTypeBuilder<DbSheetTableGroupWidgetValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableGroupRevisionId, x.SheetTableGroupWidgetId });
        builder.ToTable("SheetTableGroupWidgetValues", "spec");

        builder.Property(x => x.SheetTableGroupRevisionId)
            .HasColumnName("SheetTableGroupRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableGroupWidgetId)
            .HasColumnName("SheetTableGroupWidgetID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<WidgetValue>(v, JsonSerializerSettings.DeserializerSettings));

        builder.HasOne(x => x.SheetTableGroupWidget)
            .WithMany()
            .HasForeignKey(x => x.SheetTableGroupWidgetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
