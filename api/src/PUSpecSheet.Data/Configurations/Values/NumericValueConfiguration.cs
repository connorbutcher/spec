using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

public sealed class NumericValueConfiguration : CellValueConfiguration<NumericValue>
{
    protected override string TableName => "NumericValues";

    protected override void ConfigureValue(EntityTypeBuilder<NumericValue> builder)
    {
        // Up to 18 digits before the point and 10 after: room for large limits and fine tolerances.
        builder.Property(value => value.Value)
            .HasPrecision(28, 10);

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.NumericValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
