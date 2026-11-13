using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TemplateSectionConfiguration : IEntityTypeConfiguration<DbTemplateSection>
{
    public void Configure(EntityTypeBuilder<DbTemplateSection> builder)
    {
        ConfigureTable(builder);
        ConfigureProperties(builder);
        ConfigureIndexes(builder);
        ConfigureRelationships(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<DbTemplateSection> builder)
    {
        builder.ToTable("TemplateSections", DatabaseSchemas.Design);

        builder.HasKey(section => section.Id);
    }

    private static void ConfigureProperties(EntityTypeBuilder<DbTemplateSection> builder)
    {
        builder.Property(section => section.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(section => section.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<DbTemplateSection> builder)
    {
        // A table version has one header and only one.
        builder.HasIndex(section => section.TableTemplateVersionId, "UX_TemplateSections_OneHeaderPerVersion")
            .IsUnique()
            .HasFilter("[Role] = 'Header'");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<DbTemplateSection> builder)
    {
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
