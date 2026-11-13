using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TemplateColumnBlockConfiguration : IEntityTypeConfiguration<DbTemplateColumnBlock>
{
    public void Configure(EntityTypeBuilder<DbTemplateColumnBlock> builder)
    {
        builder.ToTable("TemplateColumnBlocks", DatabaseSchemas.Template);

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
