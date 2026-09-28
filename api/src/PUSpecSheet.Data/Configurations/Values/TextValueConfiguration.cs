using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

public sealed class TextValueConfiguration : CellValueConfiguration<TextValue>
{
    protected override string TableName => "TextValues";

    protected override void ConfigureValue(EntityTypeBuilder<TextValue> builder)
    {
        builder.Property(value => value.Value)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.TextValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
