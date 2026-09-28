using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

public sealed class BooleanValueConfiguration : CellValueConfiguration<BooleanValue>
{
    protected override string TableName => "BooleanValues";

    protected override void ConfigureValue(EntityTypeBuilder<BooleanValue> builder)
    {
        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.BooleanValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
