using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Entities.Values;

namespace PUSpecSheet.Database.Configurations.Values;

public sealed class OptionValueConfiguration : CellValueConfiguration<OptionValue>
{
    protected override string TableName => "OptionValues";

    protected override void ConfigureValue(EntityTypeBuilder<OptionValue> builder)
    {
        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.OptionValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        // An option chosen on a sheet can't be deleted; it would rewrite history.
        builder.HasOne(value => value.CellTypeOption)
            .WithMany()
            .HasForeignKey(value => value.CellTypeOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
