using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Serialization;

namespace PUSpecSheet.Api.Database.Configuration.Cell;

internal class SheetTableWidgetValueConfiguration : IEntityTypeConfiguration<DbSheetTableWidgetValue>
{
    public void Configure(EntityTypeBuilder<DbSheetTableWidgetValue> builder)
    {
        builder.HasKey(x => new { x.SheetTableRevisionId, x.SheetTableWidgetId });
        builder.ToTable("TableWidgetValues", "cell");

        builder.Property(x => x.SheetTableRevisionId)
            .HasColumnName("SheetTableRevisionID")
            .HasColumnType("int");

        builder.Property(x => x.SheetTableWidgetId)
            .HasColumnName("SheetTableWidgetID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerSettings.SerializerSettings),
                v => JsonSerializer.Deserialize<WidgetValue>(v, JsonSerializerSettings.DeserializerSettings));

        builder.HasOne(x => x.SheetTableWidget)
            .WithMany()
            .HasForeignKey(x => x.SheetTableWidgetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
