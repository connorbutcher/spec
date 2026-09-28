using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Data.Configurations;

public sealed class CellTypeOptionConfiguration : IEntityTypeConfiguration<CellTypeOption>
{
    public void Configure(EntityTypeBuilder<CellTypeOption> builder)
    {
        builder.ToTable("CellTypeOptions");

        builder.HasKey(option => option.Id);

        builder.Property(option => option.Value)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(option => new { option.CellTypeId, option.Value })
            .IsUnique();

        builder.HasOne(option => option.CellType)
            .WithMany(cellType => cellType.Options)
            .HasForeignKey(option => option.CellTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new CellTypeOption { Id = 1, CellTypeId = CellTypeSeedIds.PassFail, Value = "Pass", DisplayOrder = 1 },
            new CellTypeOption { Id = 2, CellTypeId = CellTypeSeedIds.PassFail, Value = "Fail", DisplayOrder = 2 },
            new CellTypeOption { Id = 3, CellTypeId = CellTypeSeedIds.PassFail, Value = "N/A", DisplayOrder = 3 });
    }
}
