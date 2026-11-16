using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Configurations.Sheets;

public sealed class SheetConfiguration : IEntityTypeConfiguration<DbSheet>
{
    public void Configure(EntityTypeBuilder<DbSheet> builder)
    {
        builder.ToTable("Sheets", DatabaseSchemas.Spec);

        builder.HasKey(sheet => sheet.Id);

        builder.HasPublicId(sheet => sheet.PublicId);

        builder.Property(sheet => sheet.RowVersion)
            .IsRowVersion();

        // Many sheets per phase and sheet type, told apart by their identifier values; SheetCreator keeps those unique.
        builder.HasIndex(sheet => new { sheet.PhaseId, sheet.SheetTypeId });

        builder.Property(sheet => sheet.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(sheet => sheet.Phase)
            .WithMany()
            .HasForeignKey(sheet => sheet.PhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sheet => sheet.SheetType)
            .WithMany()
            .HasForeignKey(sheet => sheet.SheetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
