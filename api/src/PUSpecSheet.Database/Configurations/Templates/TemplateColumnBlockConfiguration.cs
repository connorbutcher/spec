using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TemplateColumnBlockConfiguration : IEntityTypeConfiguration<DbTemplateColumnBlock>
{
    public void Configure(EntityTypeBuilder<DbTemplateColumnBlock> builder)
    {
        builder.ToTable("TemplateColumnBlocks", table =>
        {
            // 0 <= min <= initial <= max, and a max of at least 1.
            table.HasCheckConstraint(
                "CK_TemplateColumnBlocks_Instances",
                "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances]"
                + " AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances]))");

            table.HasCheckConstraint("CK_TemplateColumnBlocks_StickyColumnCount", "[StickyColumnCount] >= 0");
        });

        builder.HasKey(block => block.Id);

        builder.Property(block => block.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(block => block.StickyColumnCount)
            .HasDefaultValue(0);

        // Deleting a version removes its blocks; their cells go with the version's sections and rows.
        builder.HasOne(block => block.TableTemplateVersion)
            .WithMany(version => version.ColumnBlocks)
            .HasForeignKey(block => block.TableTemplateVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
