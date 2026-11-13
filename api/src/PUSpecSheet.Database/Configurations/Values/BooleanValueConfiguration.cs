using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.Database.Configurations.Values;

public sealed class BooleanValueConfiguration : CellValueConfiguration<DbBooleanValue>
{
    protected override string TableName => "BooleanValues";

    protected override void ConfigureValue(EntityTypeBuilder<DbBooleanValue> builder)
    {
        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.BooleanValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
