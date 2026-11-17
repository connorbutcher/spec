using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Configuration.Types;

public class SpecSheetTypeStructureLevelConfiguration : IEntityTypeConfiguration<DbSpecSheetTypeStructureLevel>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetTypeStructureLevel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTypeStructureLevels", "types");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTypeStructureLevelID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetTypeId)
            .HasColumnType("tinyint")
            .HasColumnName("SpecSheetTypeID");

        builder.Property(x => x.Level)
            .HasColumnType("tinyint");

        builder.Property(x => x.SortOrientation)
            .HasColumnType("tinyint");

        builder.Property(x => x.StructureLevelType)
            .HasColumnType("tinyint");

        builder.HasMany(x => x.LevelValues)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTypeStructureLevelId);
    }
}
