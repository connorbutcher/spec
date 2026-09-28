using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Data.Configurations;

public sealed class PhaseSheetTypeConfiguration : IEntityTypeConfiguration<PhaseSheetType>
{
    public void Configure(EntityTypeBuilder<PhaseSheetType> builder)
    {
        builder.ToTable("PhaseSheetTypes");

        builder.HasKey(link => new { link.PhaseId, link.SheetTypeId });

        builder.HasOne(link => link.Phase)
            .WithMany(phase => phase.SheetTypes)
            .HasForeignKey(link => link.PhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // A sheet type still selected by a phase can't be deleted.
        builder.HasOne(link => link.SheetType)
            .WithMany(sheetType => sheetType.Phases)
            .HasForeignKey(link => link.SheetTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
