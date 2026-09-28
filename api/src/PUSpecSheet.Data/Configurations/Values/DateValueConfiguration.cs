using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

public sealed class DateValueConfiguration : CellValueConfiguration<DateValue>
{
    protected override string TableName => "DateValues";

    protected override void ConfigureValue(EntityTypeBuilder<DateValue> builder)
    {
        builder.Property(value => value.Value)
            .HasColumnType("date");

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.DateValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
