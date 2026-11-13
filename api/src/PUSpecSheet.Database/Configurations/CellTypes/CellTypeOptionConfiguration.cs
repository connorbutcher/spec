using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.Database.Configurations.CellTypes;

public sealed class CellTypeOptionConfiguration : IEntityTypeConfiguration<DbCellTypeOption>
{
    public void Configure(EntityTypeBuilder<DbCellTypeOption> builder)
    {
        builder.ToTable("CellTypeOptions", DatabaseSchemas.Template);

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
    }
}
