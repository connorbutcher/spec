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
            // 0 <= min <= initial <= max, and a max of at least 1.
            table.HasCheckConstraint(
                "CK_TemplateSections_Instances",
                "[MinInstances] >= 0 AND [InitialInstances] >= [MinInstances]"
                + " AND ([MaxInstances] IS NULL OR ([MaxInstances] >= 1 AND [MaxInstances] >= [InitialInstances]))");

            // The header is top-level and always exactly one copy.
            table.HasCheckConstraint(
                "CK_TemplateSections_Header",
                "[Role] <> 'Header' OR ([ParentSectionId] IS NULL AND [MinInstances] = 1"
                + " AND [MaxInstances] = 1 AND [InitialInstances] = 1)");
        });

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(section => section.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(section => section.TableTemplateVersionId);

        // A table version has one header and only one. A named index, so it stays separate from the one above.
        builder.HasIndex(section => section.TableTemplateVersionId, "UX_TemplateSections_OneHeaderPerVersion")
            .IsUnique()
            .HasFilter("[Role] = 'Header'");

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
