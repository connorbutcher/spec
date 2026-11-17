using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Configuration.Values;

internal class NumericValueConfiguration : IEntityTypeConfiguration<DbNumericValue>
{
    public void Configure(EntityTypeBuilder<DbNumericValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("NumericValues", "values");

        builder.Property(x => x.Id)
            .HasColumnName("NumericValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableCellId)
            .HasColumnName("SpecSheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("decimal(18, 9)");
    }
}
