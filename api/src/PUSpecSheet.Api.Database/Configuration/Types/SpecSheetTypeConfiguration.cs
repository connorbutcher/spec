using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Configuration.Types;

internal class SpecSheetTypeConfiguration : IEntityTypeConfiguration<DbSpecSheetType>
{
    public void Configure(EntityTypeBuilder<DbSpecSheetType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SpecSheetTypes", "types");

        builder.Property(x => x.Id)
            .HasColumnName("SpecSheetTypeID")
            .HasColumnType("tinyint")
            .UseIdentityColumn();

        builder.Property(x => x.Type)
            .HasColumnType("varchar(64)");

        builder.Property(x => x.TableOrientation)
            .HasColumnType("varchar(16)");

        builder.Property(x => x.DisplayOrder)
            .HasColumnType("tinyint");

        builder.HasMany(x => x.TableTemplates)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTypeId);

        builder.HasMany(x => x.Properties)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTypeId);

        builder.HasMany(x => x.StructureLevels)
            .WithOne()
            .HasForeignKey(x => x.SpecSheetTypeId);
    }
}
