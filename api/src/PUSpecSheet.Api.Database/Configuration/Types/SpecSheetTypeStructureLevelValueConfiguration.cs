using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Configuration.Types;

public class SpecSheetTypeStructureLevelValueConfiguration : IEntityTypeConfiguration<DbSpecSheetTypeStructureLevelValue>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTypeStructureLevelValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTypeStructureLevelValues", "types");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTypeStructureLevelValueID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTypeStructureLevelId)
            .HasColumnName("SpecSheetTypeStructureLevelID")
            .HasColumnType("int");

        builder.Property(x => x.SpecSheetTypePropertyReferenceId)
            .HasColumnName("SpecSheetTypePropertyReferenceID")
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("tinyint");
    }
}
