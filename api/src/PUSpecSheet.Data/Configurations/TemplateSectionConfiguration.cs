using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TemplateSectionConfiguration : IEntityTypeConfiguration<TemplateSection>
{
    public void Configure(EntityTypeBuilder<TemplateSection> builder)
    {
        builder.ToTable("TemplateSections", table =>
        {
            // 0 <= min <= initial <= max, a max of at least 1, and a fixed section is at most one copy.
            table.HasCheckConstraint(
                "CK_TemplateSections_Instances",
                "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances]"
                + " AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances]))"
                + " AND ([Role] <> 'Fixed' OR [MaxInstances] = 1)");
        });

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(section => section.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Deleting a version removes all of its sections in one go.
        builder.HasOne(section => section.TableTemplateVersion)
            .WithMany(version => version.Sections)
            .HasForeignKey(section => section.TableTemplateVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // A section that still has children can't be deleted on its own. SQL Server also refuses a
        // second cascade path here, so the version cascade above is what clears nested sections.
        builder.HasOne(section => section.ParentSection)
            .WithMany(section => section.ChildSections)
            .HasForeignKey(section => section.ParentSectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
