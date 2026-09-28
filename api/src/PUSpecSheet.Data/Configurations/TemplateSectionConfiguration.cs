using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Configurations;

public sealed class TemplateSectionConfiguration : IEntityTypeConfiguration<TemplateSection>
{
    public void Configure(EntityTypeBuilder<TemplateSection> builder)
    {
        builder.ToTable("TemplateSections");

        builder.HasKey(section => section.Id);

        builder.Property(section => section.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Deleting a template removes all of its sections in one go.
        builder.HasOne(section => section.TableTemplate)
            .WithMany(template => template.Sections)
            .HasForeignKey(section => section.TableTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // A section that still has children can't be deleted on its own. SQL Server also refuses a
        // second cascade path here, so the template cascade above is what clears nested sections.
        builder.HasOne(section => section.ParentSection)
            .WithMany(section => section.ChildSections)
            .HasForeignKey(section => section.ParentSectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
