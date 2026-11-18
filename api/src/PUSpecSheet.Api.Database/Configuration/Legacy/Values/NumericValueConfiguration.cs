using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Configuration.Legacy.Values;

internal class NumericValueConfiguration : IEntityTypeConfiguration<DbNumericValue>
{
    public void Configure(EntityTypeBuilder<DbNumericValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("NumericValues", "legacy");

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
