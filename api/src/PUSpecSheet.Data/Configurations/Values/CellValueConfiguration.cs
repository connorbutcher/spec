using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Configurations.Values;

/// <summary>
/// The mapping every typed value table shares: it lives in the "values" schema, belongs to a row
/// revision (removed with it) and a template cell, and holds one value per revision and cell.
/// </summary>
public abstract class CellValueConfiguration<TValue> : IEntityTypeConfiguration<TValue>
    where TValue : class, ICellValue
{
    public void Configure(EntityTypeBuilder<TValue> builder)
    {
        builder.ToTable(TableName, DatabaseSchemas.Values);

        builder.HasKey(value => value.Id);

        builder.HasIndex(value => new { value.SheetRowRevisionId, value.TemplateCellId })
            .IsUnique();

        builder.HasOne(value => value.TemplateCell)
            .WithMany()
            .HasForeignKey(value => value.TemplateCellId)
            .OnDelete(DeleteBehavior.Restrict);

        ConfigureValue(builder);
    }

    protected abstract string TableName { get; }

    /// <summary>Maps the value column and the revision's collection navigation.</summary>
    protected abstract void ConfigureValue(EntityTypeBuilder<TValue> builder);
}
