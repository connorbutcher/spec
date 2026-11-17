using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Configuration.Values;

internal class PairValueConfiguration : IEntityTypeConfiguration<DbPairValue>
{
    public void Configure(EntityTypeBuilder<DbPairValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("PairValues", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("PairValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableCellId)
            .HasColumnName("SpecSheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.PairValueX)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.PairValueY)
            .HasColumnType("nvarchar(max)");
    }
}