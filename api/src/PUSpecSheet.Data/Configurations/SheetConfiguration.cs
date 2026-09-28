using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Configurations;

public sealed class SheetConfiguration : IEntityTypeConfiguration<Sheet>
{
    public void Configure(EntityTypeBuilder<Sheet> builder)
    {
        builder.ToTable("Sheets");

        builder.HasKey(sheet => sheet.Id);

        // One sheet per phase and sheet type.
        builder.HasIndex(sheet => new { sheet.PhaseId, sheet.SheetTypeId })
            .IsUnique();

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
