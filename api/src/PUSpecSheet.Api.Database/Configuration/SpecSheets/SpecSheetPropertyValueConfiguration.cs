using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Configuration.SpecSheets;

public class SpecSheetPropertyValueConfiguration : IEntityTypeConfiguration<DbSpecSheetPropertyValue>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetPropertyValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetPropertyValues", "legacy");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetPropertyValueID")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetId)
            .HasColumnType("int")
            .HasColumnName("SpecSheetID");

        builder.Property(x => x.SpecSheetTypePropertyId)
            .HasColumnName("SpecSheetTypePropertyID");

        builder.HasOne(x => x.SpecSheetTypeProperty)
            .WithMany()
            .HasForeignKey(x => x.SpecSheetTypePropertyId);
    }
}
