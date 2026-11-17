using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.Database.Configuration.Values;

internal class TextValueConfiguration : IEntityTypeConfiguration<DbTextValue>
{
    public void Configure(EntityTypeBuilder<DbTextValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("TextValues", "values");

        builder.Property(x => x.Id)
            .HasColumnName("TextValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTableCellId)
            .HasColumnName("SpecSheetTableCellID")
            .HasColumnType("int");

        builder.Property(x => x.Value)
            .HasColumnType("nvarchar(max)");
    }
}