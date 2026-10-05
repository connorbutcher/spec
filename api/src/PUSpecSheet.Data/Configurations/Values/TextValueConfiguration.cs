using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

public sealed class TextValueConfiguration : CellValueConfiguration<TextValue>
{
    /// <summary>
    /// The shadow property holding the start of the value, which is what a lookup by value seeks on. The
    /// value itself is too long for an index key.
    /// </summary>
    public const string LookupValue = "LookupValue";

    /// <summary>How much of the value <see cref="LookupValue"/> holds.</summary>
    public const int LookupValueLength = 200;

    protected override string TableName => "TextValues";

    protected override void ConfigureValue(EntityTypeBuilder<TextValue> builder)
    {
        builder.Property(value => value.Value)
            .IsRequired()
            .HasMaxLength(4000);

        // Computed by the database, so nothing that writes values has to know about it.
        builder.Property<string>(LookupValue)
            .HasMaxLength(LookupValueLength)
            .HasComputedColumnSql($"CONVERT(nvarchar({LookupValueLength}), LEFT([Value], {LookupValueLength}))", stored: true);

        builder.HasIndex(LookupValue)
            .IncludeProperties(nameof(TextValue.SheetCellId), nameof(TextValue.SheetRowRevisionId))
            .HasDatabaseName("IX_TextValues_LookupValue");

        builder.HasOne(value => value.SheetRowRevision)
            .WithMany(revision => revision.TextValues)
            .HasForeignKey(value => value.SheetRowRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
