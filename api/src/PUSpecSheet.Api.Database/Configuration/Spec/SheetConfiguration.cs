using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Configuration.Spec;

internal class SheetConfiguration : IEntityTypeConfiguration<DbSheet>
{
    public void Configure(EntityTypeBuilder<DbSheet> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("Sheets", "spec");

        builder.Property(x => x.Id)
            .HasColumnName("SheetID")
            .HasColumnType("int")
            .UseIdentityColumn();

        builder.Property(x => x.SpecSheetPhaseId)
            .HasColumnName("SpecSheetPhaseID")
            .HasColumnType("smallint");

        builder.Property(x => x.SpecSheetTypeId)
            .HasColumnName("SpecSheetTypeID")
            .HasColumnType("tinyint");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceID")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("newsequentialid()");

        builder.Property(x => x.CreatedDateTime)
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("sysutcdatetime()");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.SpecSheetPhase)
            .WithMany()
            .HasForeignKey(x => x.SpecSheetPhaseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.SpecSheetType)
            .WithMany()
            .HasForeignKey(x => x.SpecSheetTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tables)
            .WithOne()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Properties)
            .WithOne()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.LinkedTables)
            .WithOne()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ExternalReferences)
            .WithOne()
            .HasForeignKey(x => x.SheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
