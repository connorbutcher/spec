using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

/// <summary>
/// The mapping every typed value table shares: it lives in the "values" schema, belongs to a row
/// revision (removed with it) and a sheet cell, and holds one value per revision and cell, which is its key.
/// </summary>
public abstract class CellValueConfiguration<TValue> : IEntityTypeConfiguration<TValue>
    where TValue : class, ICellValue
{
    public void Configure(EntityTypeBuilder<TValue> builder)
    {
        builder.ToTable(TableName, DatabaseSchemas.Values);

        // A value is only ever found by its revision and cell, so that pair is the key and the table is
        // stored in that order: a revision's values sit together and are read without a second lookup.
        builder.HasKey(value => new { value.SheetRowRevisionId, value.SheetCellId });

        // Revisions and cells both sit under the row, so only the revision side cascades; the values
        // are always gone (with their revision) by the time the row's cells are removed.
        builder.HasOne(value => value.SheetCell)
            .WithMany()
            .HasForeignKey(value => value.SheetCellId)
            .OnDelete(DeleteBehavior.NoAction);

        ConfigureValue(builder);
    }

    protected abstract string TableName { get; }

    /// <summary>Maps the value column and the revision's collection navigation.</summary>
    protected abstract void ConfigureValue(EntityTypeBuilder<TValue> builder);
}
