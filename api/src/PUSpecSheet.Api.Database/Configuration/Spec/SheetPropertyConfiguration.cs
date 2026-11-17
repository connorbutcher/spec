using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetPropertyConfiguration : IEntityTypeConfiguration<DbSheetProperty>
{
    public void Configure(EntityTypeBuilder<DbSheetProperty> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("SheetProperties", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetPropertyID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SheetId)
            .HasColumnName("SheetID")
            .HasColumnType("int");

        builder.Property(x => x.SpecSheetTypePropertyId)
            .HasColumnName("SpecSheetTypePropertyID")
            .HasColumnType("int");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.HasOne(x => x.SpecSheetTypeProperty)
            .WithMany()
            .HasForeignKey(x => x.SpecSheetTypePropertyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SheetPropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
