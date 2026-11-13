using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Values;
using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Database.Configurations.Values;

public sealed class LabelledTextValueConfiguration : CellValueConfiguration<DbLabelledTextValue>
{
    /// <summary>The shadow property holding the start of the value, which a lookup by value seeks on, as for text values.</summary>
    public const string LookupValue = TextValueConfiguration.LookupValue;

    protected override string TableName => "LabelledTextValues";

    /// <summary>A labelled text cell holds one value for each of its slots.</summary>
    protected override void ConfigureKey(EntityTypeBuilder<DbLabelledTextValue> builder)
    {
        builder.HasKey(value => new { value.SheetRowRevisionId, value.SheetCellId, value.SlotKey });
    }

    protected override void ConfigureValue(EntityTypeBuilder<DbLabelledTextValue> builder)
    {
        builder.Property(value => value.SlotKey)
            .IsRequired()
            .HasMaxLength(LabelledTextRules.LongestKey)
            .IsUnicode(false);

        builder.Property(value => value.Value)
            .IsRequired()
            .HasMaxLength(4000);

        // Computed by the database, so nothing that writes values has to know about it.
        var length = TextValueConfiguration.LookupValueLength;
        builder.Property<string>(LookupValue)
            .HasMaxLength(length)
            .HasComputedColumnSql($"CONVERT(nvarchar({length}), LEFT([Value], {length}))", stored: true);

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.LabelledTextValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
