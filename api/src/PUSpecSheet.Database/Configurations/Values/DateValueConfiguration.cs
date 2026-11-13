using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.Database.Configurations.Values;

public sealed class DateValueConfiguration : CellValueConfiguration<DbDateValue>
{
    protected override string TableName => "DateValues";

    protected override void ConfigureValue(EntityTypeBuilder<DbDateValue> builder)
    {
        builder.Property(value => value.Value)
            .HasColumnType("date");

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.DateValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
