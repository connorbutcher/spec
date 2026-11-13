using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PUSpecSheet.Database.Configurations.Templates;

public sealed class TemplateRowConfiguration : IEntityTypeConfiguration<DbTemplateRow>
{
    public void Configure(EntityTypeBuilder<DbTemplateRow> builder)
    {
        builder.ToTable("TemplateRows");

        builder.HasKey(row => row.Id);

        builder.HasOne(row => row.TemplateSection)
            .WithMany(section => section.Rows)
            .HasForeignKey(row => row.TemplateSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
