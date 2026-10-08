using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Data.Conversions;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

public sealed class CellSettingsValueConfiguration : CellValueConfiguration<CellSettingsValue>
{
    protected override string TableName => "CellSettings";

    protected override void ConfigureValue(EntityTypeBuilder<CellSettingsValue> builder)
    {
        // JSON, so a new setting never needs a new column.
        builder.Property(value => value.Settings)
            .IsRequired()
            .HasConversion<CellInstanceSettingsConverter>();

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.CellSettings)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
